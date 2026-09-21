using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SodiqCafeMVC.Application.DTOs;

namespace SodiqCafeMVC.Application.Interfaces
{
    public interface IBillingService
    {
        // 1. Oyning 1-sanasidan oxirgi sanasigacha bo'lgan davr uchun invoys yaratish
        Task<InvoiceDto> GenerateMonthlyInvoiceAsync(int cafeId, DateTime? targetDate = null);

        // 2. Barcha kafelar uchun oylik invoyslarni chiqarish (har oy oxirida)
        Task<List<InvoiceDto>> GenerateAllMonthlyInvoicesAsync(DateTime? targetDate = null);

        // 3. Owner balansini to'ldirish va uning barcha kafelari uchun avtomatik ravishda to'lovni yechish
        Task<bool> TopUpBalanceAndProcessPaymentAsync(int userId, decimal amount, string? note = null);

        // 4. Invoys bo'yicha balansdan avtomatik to'lov qilish (ortiqchasi balansda qoladi, muddat 1 oyga uzayadi)
        Task<bool> ProcessInvoicePaymentAsync(int invoiceId, bool isExternalPayment = false);

        // 5. To'lov qilmagan va faollik muddati o'tgan kafelarni vaqtincha to'xtatish (Suspend)
        Task CheckAndSuspendOverdueCafesAsync();

        // 6. Invoyslar ro'yxati
        Task<List<InvoiceDto>> GetAllInvoicesAsync();
        Task<List<InvoiceDto>> GetCafeInvoicesAsync(int cafeId);

        // 7. Admin Dashboard statistikasi
        Task<BillingSummaryDto> GetBillingSummaryAsync();

        // 8. Kafeni qo'lda to'xtatish yoki faollashtirish
        Task<bool> ToggleCafeSuspendStatusAsync(int cafeId);

        // 9. Faollik muddatini qo'lda uzaytirish
        Task<bool> ExtendActiveUntilManuallyAsync(int cafeId, int months);
    }
}
