using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SodiqCafeMVC.Application.DTOs;
using SodiqCafeMVC.Application.Interfaces;
using SodiqCafeMVC.Domain.Enums;
using SodiqCafeMVC.Infrastructure.Data;
using SodiqCafeMVC.Domain.Entities;

namespace SodiqCafeMVC.Web.ApiControllers;

[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminApiController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly IBillingService _billingService;
    private readonly AppDbContext _context;

    public AdminApiController(IAuthService authService, IBillingService billingService, AppDbContext context)
    {
        _authService = authService;
        _billingService = billingService;
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var activeCafes = await _context.Cafes.CountAsync(c => !c.IsSuspended && c.ActiveUntil > DateTime.UtcNow);
        var totalOwners = await _context.Users.CountAsync(u => u.Role == Role.CafeOwner);
        var pendingInvoicesCount = await _context.Invoices.CountAsync(i => i.Status == InvoiceStatus.Pending);

        return ApiOk(new { activeCafes, totalOwners, pendingInvoicesCount });
    }

    [HttpGet("owners")]
    public async Task<IActionResult> Owners()
    {
        var owners = await _context.Users
            .Where(u => u.Role == Role.CafeOwner)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.Balance,
                CafesCount = u.Cafes.Count
            })
            .ToListAsync();
        return ApiOk(owners);
    }

    [HttpGet("owners/{id}")]
    public async Task<IActionResult> OwnerDetail(int id)
    {
        var owner = await _context.Users
            .Include(u => u.Cafes)
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == Role.CafeOwner);

        if (owner == null) return ApiFail("Topilmadi", 404);

        return ApiOk(new
        {
            owner.Id, owner.FullName, owner.Email, owner.PhoneNumber, owner.Balance,
            Cafes = owner.Cafes.Select(c => new { c.Id, c.Name, c.ActiveUntil, c.IsSuspended })
        });
    }

    [HttpPost("owners")]
    public async Task<IActionResult> RegisterOwner([FromBody] RegisterOwnerDto model)
    {
        if (!ModelState.IsValid) return ApiFail("Noto'g'ri so'rov");

        int adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _authService.RegisterCafeOwnerAsync(model, adminId);
        if (!result.Success) return ApiFail(result.Message!);

        return ApiOk(result.User, "Kafe egasi muvaffaqiyatli qo'shildi");
    }

    [HttpPost("owners/{id}/cafes")]
    public async Task<IActionResult> AddCafe(int id, [FromBody] RegisterCafeDto model)
    {
        if (!ModelState.IsValid) return ApiFail("Noto'g'ri so'rov");
        
        int adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _authService.AddCafeToOwnerAsync(id, model, adminId);
        
        if (!result.Success) return ApiFail(result.Message!);
        return ApiOk(result.User, "Kafe qo'shildi");
    }

    [HttpGet("cafes")]
    public async Task<IActionResult> Cafes()
    {
        var cafes = await _context.Cafes
            .Include(c => c.Owner)
            .Select(c => new { c.Id, c.Name, OwnerName = c.Owner.FullName, c.ActiveUntil, c.IsSuspended })
            .ToListAsync();
        return ApiOk(cafes);
    }

    [HttpPost("cafes/{id}/toggle-suspend")]
    public async Task<IActionResult> ToggleSuspend(int id)
    {
        var success = await _billingService.ToggleCafeSuspendStatusAsync(id);
        if (!success) return ApiFail("Kafe topilmadi yoki xatolik");
        return ApiOk<object>(null, "Holat o'zgardi");
    }

    [HttpPost("cafes/{id}/extend")]
    public async Task<IActionResult> ExtendActiveUntil(int id, [FromBody] ExtendCafeRequest req)
    {
        var success = await _billingService.ExtendActiveUntilManuallyAsync(id, req.Months);
        if (!success) return ApiFail("Xatolik");
        return ApiOk<object>(null, "Muddat uzaytirildi");
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> Invoices()
    {
        var invoices = await _context.Invoices
            .Include(i => i.Cafe)
            .OrderByDescending(i => i.CreatedAt)
            .Take(50)
            .Select(i => new { i.Id, CafeName = i.Cafe.Name, i.Amount, PeriodStart = i.PeriodStart, PeriodEnd = i.PeriodEnd, i.Status, i.CreatedAt })
            .ToListAsync();
        return ApiOk(invoices);
    }

    [HttpPost("invoices/generate")]
    public async Task<IActionResult> GenerateMonthlyInvoices([FromBody] GenerateInvoicesRequest req)
    {
        DateTime targetDate = req.Year.HasValue && req.Month.HasValue
            ? new DateTime(req.Year.Value, req.Month.Value, 1)
            : DateTime.UtcNow;

        var result = await _billingService.GenerateAllMonthlyInvoicesAsync(targetDate);
        return ApiOk(new { Count = result.Count }, $"{result.Count} ta faktura yaratildi");
    }

    [HttpPost("invoices/{id}/pay")]
    public async Task<IActionResult> PayInvoice(int id)
    {
        var success = await _billingService.ProcessInvoicePaymentAsync(id);
        if (!success) return ApiFail("To'lovni amalga oshirib bo'lmadi");
        return ApiOk<object>(null, "To'lov qabul qilindi");
    }

    [HttpPost("owners/{id}/topup")]
    public async Task<IActionResult> TopUpBalance(int id, [FromBody] TopupRequest req)
    {
        if (req.Amount <= 0) return ApiFail("Noto'g'ri summa");
        var success = await _billingService.TopUpBalanceAndProcessPaymentAsync(id, req.Amount, req.Note);
        if (!success) return ApiFail("Xatolik yuz berdi");
        return ApiOk<object>(null, "Balans to'ldirildi");
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Settings()
    {
        var settings = await _context.SystemSettings.FirstOrDefaultAsync();
        return ApiOk(settings);
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] SystemSettings req)
    {
        var settings = await _context.SystemSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = req;
            _context.SystemSettings.Add(settings);
        }
        else
        {
            settings.BasePrice = req.BasePrice;
            settings.BaseUsersLimit = req.BaseUsersLimit;
            settings.PricePerExtraUsers = req.PricePerExtraUsers;
            settings.ExtraUsersStep = req.ExtraUsersStep;
        }
        await _context.SaveChangesAsync();
        return ApiOk(settings, "Sozlamalar yangilandi");
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> Sessions()
    {
        var sessions = await _authService.GetAllActiveSessionsAsync();
        return ApiOk(sessions);
    }

    [HttpDelete("sessions/{sessionId}")]
    public async Task<IActionResult> RevokeSession(string sessionId)
    {
        await _authService.RevokeSessionAsync(sessionId);
        return ApiOk<object>(null, "Sessiya bekor qilindi");
    }
}

public class ExtendCafeRequest { public int Months { get; set; } = 1; }
public class GenerateInvoicesRequest { public int? Year { get; set; } public int? Month { get; set; } }
public class TopupRequest { public decimal Amount { get; set; } public string? Note { get; set; } }
