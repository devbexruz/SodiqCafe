using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SodiqCafeMVC.Application.DTOs;
using SodiqCafeMVC.Application.Interfaces;
using SodiqCafeMVC.Domain.Entities;
using SodiqCafeMVC.Domain.Enums;
using SodiqCafeMVC.Infrastructure.Data;

namespace SodiqCafeMVC.Infrastructure.Services
{
    public class BillingService : IBillingService
    {
        private readonly AppDbContext _context;

        public BillingService(AppDbContext context)
        {
            _context = context;
        }

        // Oyning 1-sanasidan oxirgi sanasigacha hisoblab Invoys yaratish
        public async Task<InvoiceDto> GenerateMonthlyInvoiceAsync(int cafeId, DateTime? targetDate = null)
        {
            var cafe = await _context.Cafes
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.Id == cafeId);

            if (cafe == null)
            {
                throw new InvalidOperationException($"#{cafeId} raqamli kafe topilmadi.");
            }

            var date = targetDate ?? DateTime.UtcNow;
            var periodStart = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var periodEnd = new DateTime(date.Year, date.Month, daysInMonth, 23, 59, 59, DateTimeKind.Utc);

            // Ushbu davr uchun allaqachon invoys mavjud bo'lsa
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.CafeId == cafeId && i.PeriodStart == periodStart);

            if (existingInvoice != null)
            {
                return MapToDto(existingInvoice, cafe);
            }
            
            // Oy davomida active bo'lgan qo'shimcha userlar sonini hisoblash (misol uchun 100 tadan oshsa)
            // Bu yerda qo'shimcha to'lov mantiqini qo'shamiz
            int activeUsersInPeriod = await _context.CafeUsers
                .Where(cu => cu.CafeId == cafeId && cu.LastActivityAt >= periodStart && cu.LastActivityAt <= periodEnd)
                .CountAsync();
            
            var activeUsers = await _context.CafeUsers.CountAsync(c => c.CafeId == cafe.Id);

            decimal basePrice = cafe.BasePrice;
            int baseLimit = cafe.BaseUsersLimit;
            decimal pricePerExtra = cafe.PricePerExtraUsers;
            int extraStep = cafe.ExtraUsersStep;

            if (!cafe.HasCustomPrice)
            {
                var settings = await _context.SystemSettings.FirstOrDefaultAsync();
                if (settings != null)
                {
                    basePrice = settings.BasePrice;
                    baseLimit = settings.BaseUsersLimit;
                    pricePerExtra = settings.PricePerExtraUsers;
                    extraStep = settings.ExtraUsersStep;
                }
            }

            decimal extraUsersCount = Math.Max(0, activeUsers - baseLimit);
            decimal extraMultiplier = Math.Ceiling(extraUsersCount / extraStep);
            decimal totalFee = basePrice + (extraMultiplier * pricePerExtra);

            var invoiceNumber = $"INV-{date:yyyyMM}-{cafe.Id:D3}";
            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                CafeId = cafe.Id,
                Amount = totalFee,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                DueDate = periodEnd,
                Status = InvoiceStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Notes = $"{date:yyyy-MMMM} oyi uchun abonent to'lovi. Active mijozlar: {activeUsersInPeriod}"
            };

            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();

            // Agar ownerning balansi yetarli bo'lsa, darhol avtomatik pul yechiladi va muddat 1 oyga uzaytiriladi!
            if (cafe.Owner.Balance >= invoice.Amount)
            {
                await ProcessInvoicePaymentAsync(invoice.Id);
            }

            return MapToDto(invoice, cafe);
        }

        // Barcha kafelar uchun oylik invoyslarni chiqarish
        public async Task<List<InvoiceDto>> GenerateAllMonthlyInvoicesAsync(DateTime? targetDate = null)
        {
            var cafes = await _context.Cafes.ToListAsync();
            var results = new List<InvoiceDto>();

            foreach (var cafe in cafes)
            {
                var invoice = await GenerateMonthlyInvoiceAsync(cafe.Id, targetDate);
                results.Add(invoice);
            }

            return results;
        }

        // Owner balansini to'ldirish va uning barcha kafelari uchun avtomatik ravishda to'lovni yechish
        public async Task<bool> TopUpBalanceAndProcessPaymentAsync(int userId, decimal amount, string? note = null)
        {
            if (amount <= 0) return false;

            var user = await _context.Users
                .Include(u => u.Cafes)
                .ThenInclude(c => c.Invoices)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            // 1. Balansni to'ldirish
            user.Balance += amount;

            var transaction = new PaymentTransaction
            {
                UserId = user.Id,
                Amount = amount,
                Type = PaymentType.TopUp,
                CreatedAt = DateTime.UtcNow,
                Description = note ?? $"Balans {amount:N0} so'mga to'ldirildi"
            };
            await _context.Transactions.AddAsync(transaction);
            await _context.SaveChangesAsync();

            // 2. To'lanmagan ochiq invoyslar bo'lsa, avtomatik pul yechib borish
            foreach (var cafe in user.Cafes)
            {
                var pendingInvoices = cafe.Invoices
                    .Where(i => i.Status == InvoiceStatus.Pending)
                    .OrderBy(i => i.PeriodStart)
                    .ToList();

                foreach (var invoice in pendingInvoices)
                {
                    if (user.Balance >= invoice.Amount)
                    {
                        await ProcessInvoicePaymentInternalAsync(cafe, invoice, user);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Muayyan invoysni balansdan to'lash
        public async Task<bool> ProcessInvoicePaymentAsync(int invoiceId, bool isExternalPayment = false)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Cafe)
                .ThenInclude(c => c.Owner)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null || invoice.Status == InvoiceStatus.Paid)
                return false;

            var cafe = invoice.Cafe;
            var owner = cafe.Owner;
            
            if (isExternalPayment) 
            {
                // To'lov chek orqali qilingan bo'lsa (masalan Telegram admin orqali),
                // uni hisobiga shu miqdorda pul tushirib, so'ngra yechamiz
                owner.Balance += invoice.Amount;
                
                // To'lov tranzaksiyasi yozib qoyish ham mumkin
                var transaction = new PaymentTransaction
                {
                    UserId = owner.Id,
                    Amount = invoice.Amount,
                    Type = PaymentType.TopUp,
                    CreatedAt = DateTime.UtcNow,
                    Description = $"Telegram orqali chek bilan tasdiqlandi (Invoys #{invoice.InvoiceNumber})"
                };
                await _context.Transactions.AddAsync(transaction);
            }
            else if (owner.Balance < invoice.Amount)
            {
                return false;
            }

            await ProcessInvoicePaymentInternalAsync(cafe, invoice, owner);
            await _context.SaveChangesAsync();
            return true;
        }

        // Ichki pul yechish mantiqi: Balansdan avto yechiladi, ortiqchasi qoladi, muddat 1 oyga uzaytiriladi
        private async Task ProcessInvoicePaymentInternalAsync(Cafe cafe, Invoice invoice, User owner)
        {
            // Balansdan yechiladi, ortiqchasi balansda qoladi
            owner.Balance -= invoice.Amount;

            // Invoys holatini yangilash
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;

            // Kafening faollik muddatini 1 oyga uzaytirish
            var baseDate = cafe.ActiveUntil > DateTime.UtcNow ? cafe.ActiveUntil : DateTime.UtcNow;
            cafe.ActiveUntil = baseDate.AddMonths(1);

            // To'xtatilgan bo'lsa, yana faollashtirish
            cafe.IsSuspended = false;

            // Tranzaksiyani qayd qilish
            var deduction = new PaymentTransaction
            {
                CafeId = cafe.Id,
                InvoiceId = invoice.Id,
                Amount = -invoice.Amount,
                Type = PaymentType.AutoDeduction,
                CreatedAt = DateTime.UtcNow,
                Description = $"{invoice.InvoiceNumber} invoysi uchun avtomatik to'lov. Yangi muddat: {cafe.ActiveUntil:dd.MM.yyyy}"
            };

            await _context.Transactions.AddAsync(deduction);
        }

        // To'lov qilmagan va faollik muddati o'tgan kafelarni vaqtincha to'xtatish (Suspend)
        public async Task CheckAndSuspendOverdueCafesAsync()
        {
            var now = DateTime.UtcNow;
            var overdueCafes = await _context.Cafes
                .Where(c => c.ActiveUntil < now && !c.IsSuspended)
                .ToListAsync();

            foreach (var cafe in overdueCafes)
            {
                cafe.IsSuspended = true;
            }

            if (overdueCafes.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<InvoiceDto>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Cafe)
                .ThenInclude(c => c.Owner)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => MapToDto(i, i.Cafe))
                .ToListAsync();
        }

        public async Task<List<InvoiceDto>> GetCafeInvoicesAsync(int cafeId)
        {
            return await _context.Invoices
                .Include(i => i.Cafe)
                .ThenInclude(c => c.Owner)
                .Where(i => i.CafeId == cafeId)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => MapToDto(i, i.Cafe))
                .ToListAsync();
        }

        public async Task<BillingSummaryDto> GetBillingSummaryAsync()
        {
            var now = DateTime.UtcNow;
            var totalCafes = await _context.Cafes.CountAsync();
            var activeCafes = await _context.Cafes.CountAsync(c => !c.IsSuspended && c.ActiveUntil >= now);
            var suspendedCafes = await _context.Cafes.CountAsync(c => c.IsSuspended || c.ActiveUntil < now);

            var pendingInvoices = await _context.Invoices.Where(i => i.Status == InvoiceStatus.Pending).ToListAsync();
            var paidInvoices = await _context.Invoices.Where(i => i.Status == InvoiceStatus.Paid).ToListAsync();

            var totalPendingAmount = pendingInvoices.Sum(i => i.Amount);
            var totalPaidAmount = paidInvoices.Sum(i => i.Amount);
            var totalBalance = await _context.Users.Where(u => u.Role == Role.CafeOwner).SumAsync(u => u.Balance);

            return new BillingSummaryDto
            {
                TotalCafes = totalCafes,
                ActiveCafes = activeCafes,
                SuspendedCafes = suspendedCafes,
                PendingInvoicesCount = pendingInvoices.Count,
                TotalPendingInvoicesAmount = totalPendingAmount,
                TotalPaidAmount = totalPaidAmount,
                TotalCafesBalance = totalBalance
            };
        }

        public async Task<bool> ToggleCafeSuspendStatusAsync(int cafeId)
        {
            var cafe = await _context.Cafes.FindAsync(cafeId);
            if (cafe == null) return false;

            cafe.IsSuspended = !cafe.IsSuspended;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExtendActiveUntilManuallyAsync(int cafeId, int months)
        {
            if (months <= 0) return false;

            var cafe = await _context.Cafes.FindAsync(cafeId);
            if (cafe == null) return false;

            var baseDate = cafe.ActiveUntil > DateTime.UtcNow ? cafe.ActiveUntil : DateTime.UtcNow;
            cafe.ActiveUntil = baseDate.AddMonths(months);
            cafe.IsSuspended = false;

            await _context.SaveChangesAsync();
            return true;
        }

        private static InvoiceDto MapToDto(Invoice i, Cafe c)
        {
            return new InvoiceDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                CafeId = i.CafeId,
                CafeName = c.Name,
                OwnerName = c.Owner != null ? (c.Owner.FullName ?? c.Owner.Username) : "N/A",
                Amount = i.Amount,
                PeriodStart = i.PeriodStart,
                PeriodEnd = i.PeriodEnd,
                DueDate = i.DueDate,
                Status = i.Status,
                CreatedAt = i.CreatedAt,
                PaidAt = i.PaidAt,
                Notes = i.Notes,
                CanBePaid = i.Status == InvoiceStatus.Pending && (c.Owner?.Balance ?? 0) >= i.Amount
            };
        }
    }
}
