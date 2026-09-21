using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SodiqCafeMVC.Application.DTOs;
using SodiqCafeMVC.Application.Interfaces;

namespace SodiqCafeMVC.Web.ApiControllers
{
    [ApiController]
    [Route("api/billing")]
    [Authorize(Roles = "Admin")]
    public class BillingApiController : ControllerBase
    {
        private readonly IBillingService _billingService;

        public BillingApiController(IBillingService billingService)
        {
            _billingService = billingService;
        }

        // 1. Umumiy dashboard statistikasi
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _billingService.GetBillingSummaryAsync();
            return Ok(summary);
        }

        // 2. Barcha invoyslar
        [HttpGet("invoices")]
        public async Task<IActionResult> GetInvoices([FromQuery] int? cafeId = null)
        {
            var invoices = cafeId.HasValue
                ? await _billingService.GetCafeInvoicesAsync(cafeId.Value)
                : await _billingService.GetAllInvoicesAsync();

            return Ok(invoices);
        }

        // 3. Oylik invoyslarni chiqarish (1-sanadan oxirgi sanagacha)
        [HttpPost("invoices/generate-monthly")]
        public async Task<IActionResult> GenerateMonthly([FromBody] GenerateMonthlyRequest? request)
        {
            DateTime targetDate;
            if (request != null && request.Year > 0 && request.Month > 0)
            {
                targetDate = new DateTime(request.Year, request.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            }
            else
            {
                targetDate = DateTime.UtcNow;
            }

            var invoices = await _billingService.GenerateAllMonthlyInvoicesAsync(targetDate);
            return Ok(new
            {
                success = true,
                message = $"{targetDate:yyyy-MMMM} oyi uchun {invoices.Count} ta invoys chiqarildi.",
                invoices
            });
        }

        // 4. Kafe balansini to'ldirish (Pul kiritiladi, avtomatik pul yechiladi, muddat 1 oyga uzaytiriladi, ortiqchasi qoladi)
        [HttpPost("cafes/{cafeId:int}/topup")]
        public async Task<IActionResult> TopUpBalance(int cafeId, [FromBody] TopUpBalanceDto dto)
        {
            if (dto.Amount <= 0)
            {
                return BadRequest(new { success = false, message = "Summa 0 dan katta bo'lishi shart." });
            }

            var success = await _billingService.TopUpBalanceAndProcessPaymentAsync(cafeId, dto.Amount, dto.Note);
            if (!success)
            {
                return BadRequest(new { success = false, message = "Balansni to'ldirish amalga oshmadi." });
            }

            return Ok(new
            {
                success = true,
                message = $"Kafe balansi {dto.Amount:N0} so'mga to'ldirildi. Tizim avtomatik to'lovni yechdi va ortiqchasi balansda qoldi."
            });
        }

        // 5. Invoysni balansdan to'lash
        [HttpPost("invoices/{invoiceId:int}/pay")]
        public async Task<IActionResult> PayInvoice(int invoiceId)
        {
            var success = await _billingService.ProcessInvoicePaymentAsync(invoiceId);
            if (!success)
            {
                return BadRequest(new { success = false, message = "To'lov amalga oshmadi. Balans yetarli emas yoki invoys allaqachon to'langan." });
            }

            return Ok(new
            {
                success = true,
                message = "Invoys bo'yicha to'lov balansdan yechildi va faollik muddati 1 oyga uzaytirildi."
            });
        }

        // 6. Kafeni vaqtincha to'xtatish yoki qayta yoqish
        [HttpPost("cafes/{cafeId:int}/toggle-suspend")]
        public async Task<IActionResult> ToggleSuspend(int cafeId)
        {
            var success = await _billingService.ToggleCafeSuspendStatusAsync(cafeId);
            if (!success)
            {
                return NotFound(new { success = false, message = "Kafe topilmadi." });
            }

            return Ok(new
            {
                success = true,
                message = "Kafening ishlash holati muvaffaqiyatli o'zgartirildi."
            });
        }
    }

    public class GenerateMonthlyRequest
    {
        public int Year { get; set; }
        public int Month { get; set; }
    }
}
