using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SodiqCafeMVC.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using SodiqCafeMVC.Application.Interfaces;
using SodiqCafeMVC.Infrastructure.Data;
using SodiqCafeMVC.Domain.Entities;
using SodiqCafeMVC.Domain.Enums;

namespace SodiqCafeMVC.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IBillingService _billingService;

        public AdminController(IAuthService authService, IBillingService billingService)
        {
            _authService = authService;
            _billingService = billingService;
        }

        // Shablon mahsulotlarni boshqarish
        [HttpGet("templates")]
        public async Task<IActionResult> Templates([FromServices] AppDbContext context)
        {
            var templates = await context.Products
                .Where(p => p.IsDefaultTemplate)
                .OrderBy(p => p.Name)
                .ToListAsync();
            
            return View(templates);
        }

        [HttpPost("templates/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTemplate(string name, decimal price, ProductCategory category, Microsoft.AspNetCore.Http.IFormFile? imageFile, [FromServices] AppDbContext context, [FromServices] SodiqCafeMVC.Application.Interfaces.IFileStorageService fileStorageService)
        {
            string? imageUrl = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                using var stream = imageFile.OpenReadStream();
                imageUrl = await fileStorageService.UploadFileAsync(stream, imageFile.FileName, imageFile.ContentType);
            }

            var template = new Product
            {
                Name = name,
                Price = price,
                Category = category,
                ImageUrl = imageUrl,
                IsDefaultTemplate = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Products.AddAsync(template);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yangi shablon muvaffaqiyatli qo'shildi.";
            return RedirectToAction(nameof(Templates));
        }

        [HttpPost("templates/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTemplate(int templateId, [FromServices] AppDbContext context)
        {
            var template = await context.Products.FindAsync(templateId);
            if (template != null && template.IsDefaultTemplate)
            {
                context.Products.Remove(template);
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Shablon o'chirildi.";
            }

            return RedirectToAction(nameof(Templates));
        }

        [HttpPost("templates/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTemplate(int templateId, string name, decimal price, ProductCategory category, Microsoft.AspNetCore.Http.IFormFile? imageFile, [FromServices] AppDbContext context, [FromServices] SodiqCafeMVC.Application.Interfaces.IFileStorageService fileStorageService)
        {
            var template = await context.Products.FindAsync(templateId);
            if (template == null || !template.IsDefaultTemplate)
            {
                return NotFound();
            }

            template.Name = name;
            template.Price = price;
            template.Category = category;

            if (imageFile != null && imageFile.Length > 0)
            {
                using var stream = imageFile.OpenReadStream();
                template.ImageUrl = await fileStorageService.UploadFileAsync(stream, imageFile.FileName, imageFile.ContentType);
            }

            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Shablon muvaffaqiyatli tahrirlandi.";
            return RedirectToAction(nameof(Templates));
        }

        // Kafelar va ularning faolligi, balansi boshqaruvi
        [HttpGet("")]
        [HttpGet("cafes")]
        public async Task<IActionResult> Cafes()
        {
            // Muddati o'tgan kafelarni tekshirib to'xtatib qo'yish
            await _billingService.CheckAndSuspendOverdueCafesAsync();

            var cafes = await _authService.GetAllCafesAsync();
            var summary = await _billingService.GetBillingSummaryAsync();

            ViewBag.Summary = summary;
            return View(cafes);
        }
        // ==========================================
        // KAFE DETALLARI VA SOZLAMALARI
        // ==========================================

        [HttpGet("cafes/{id}")]
        public async Task<IActionResult> CafeDetail(int id, [FromServices] AppDbContext context)
        {
            var cafe = await context.Cafes
                .Include(c => c.Owner)
                .Include(c => c.Products)
                .Include(c => c.CafeUsers)
                .Include(c => c.Invoices)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            if (cafe == null) return NotFound();
            
            return View(cafe);
        }

        [HttpPost("cafes/update-details")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCafeDetails(int cafeId, string name, string? address, string? phoneNumber, Microsoft.AspNetCore.Http.IFormFile? logoFile, [FromServices] AppDbContext context, [FromServices] SodiqCafeMVC.Application.Interfaces.IFileStorageService fileStorageService)
        {
            var cafe = await context.Cafes.FirstOrDefaultAsync(c => c.Id == cafeId);
            if (cafe == null) return NotFound();

            cafe.Name = name;
            cafe.Address = address;
            cafe.PhoneNumber = phoneNumber;

            if (logoFile != null && logoFile.Length > 0)
            {
                using var stream = logoFile.OpenReadStream();
                var newLogoUrl = await fileStorageService.UploadFileAsync(stream, logoFile.FileName, logoFile.ContentType);

                if (!string.IsNullOrEmpty(cafe.LogoUrl))
                {
                    await fileStorageService.DeleteFileAsync(cafe.LogoUrl);
                }

                cafe.LogoUrl = newLogoUrl;
            }

            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kafe ma'lumotlari muvaffaqiyatli yangilandi.";
            
            return RedirectToAction(nameof(CafeDetail), new { id = cafeId });
        }
        // ==========================================
        // OWNERS
        // ==========================================

        [HttpGet("owners")]
        public async Task<IActionResult> Owners([FromServices] AppDbContext context)
        {
            var owners = await context.Users
                .Where(u => u.Role == Role.CafeOwner)
                .Include(u => u.Cafes)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(owners);
        }

        [HttpGet("owners/{id}")]
        public async Task<IActionResult> OwnerDetail(int id, [FromServices] AppDbContext context)
        {
            var owner = await context.Users
                .Include(u => u.Cafes)
                    .ThenInclude(c => c.Invoices)
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == Role.CafeOwner);

            if (owner == null)
                return NotFound();

            return View(owner);
        }

        // ------------------ OWNER & CAFE REGISTRATION ------------------

        // Kafe egasini ro'yxatdan o'tkazish formasi
        [HttpGet("register-owner")]
        public IActionResult RegisterOwner()
        {
            return View(new RegisterOwnerDto());
        }

        // Kafe egasini ro'yxatdan o'tkazish (Faqat Admin)
        [HttpPost("register-owner")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterOwner(RegisterOwnerDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(adminIdClaim, out var adminId))
            {
                ModelState.AddModelError(string.Empty, "Admin identifikatsiyasida xatolik yuz berdi.");
                return View(model);
            }

            var result = await _authService.RegisterCafeOwnerAsync(model, adminId);
            
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Ro'yxatdan o'tkazishda xatolik yuz berdi.");
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Owners)); // Go to Owners list page
        }

        // Yangi kafe qo'shish formasi
        [HttpGet("add-cafe/{ownerId}")]
        public IActionResult AddCafe(int ownerId)
        {
            ViewBag.OwnerId = ownerId;
            return View(new RegisterCafeDto { OwnerId = ownerId });
        }

        // Mavjud egasiga yangi kafe qo'shish
        [HttpPost("add-cafe/{ownerId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCafe(int ownerId, RegisterCafeDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.OwnerId = ownerId;
                return View(model);
            }

            var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(adminIdClaim, out var adminId))
            {
                ModelState.AddModelError(string.Empty, "Admin identifikatsiyasida xatolik yuz berdi.");
                ViewBag.OwnerId = ownerId;
                return View(model);
            }

            model.OwnerId = ownerId;
            var result = await _authService.AddCafeToOwnerAsync(ownerId, model, adminId);
            
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Kafe qo'shishda xatolik yuz berdi.");
                ViewBag.OwnerId = ownerId;
                return View(model);
            }

            // Yangi ro'yxatdan o'tgan kafe uchun joriy oy invoysini ham yaratib qo'yamiz
            if (result.User != null && result.User.Cafes != null)
            {
                var newCafe = result.User.Cafes.LastOrDefault();
                if (newCafe != null)
                {
                    await _billingService.GenerateMonthlyInvoiceAsync(newCafe.Id);
                }
            }

            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(OwnerDetail), new { id = ownerId });
        }

        // ==========================================
        // BILLING & INVOICES
        // ==========================================

        // Barcha invoyslar ro'yxati
        [HttpGet("invoices")]
        public async Task<IActionResult> Invoices([FromQuery] int? cafeId = null)
        {
            var invoices = cafeId.HasValue
                ? await _billingService.GetCafeInvoicesAsync(cafeId.Value)
                : await _billingService.GetAllInvoicesAsync();

            var summary = await _billingService.GetBillingSummaryAsync();
            ViewBag.Summary = summary;
            ViewBag.SelectedCafeId = cafeId;

            return View(invoices);
        }

        // Har oy oxirida/boshida barcha kafelar uchun oylik invoyslarni chiqarish (1-sanadan oxirgi sanagacha)
        [HttpPost("invoices/generate-monthly")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateMonthlyInvoices([FromForm] int? year = null, [FromForm] int? month = null)
        {
            DateTime targetDate;
            if (year.HasValue && month.HasValue)
            {
                targetDate = new DateTime(year.Value, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            }
            else
            {
                targetDate = DateTime.UtcNow;
            }

            var invoices = await _billingService.GenerateAllMonthlyInvoicesAsync(targetDate);
            TempData["SuccessMessage"] = $"{targetDate:yyyy-MMMM} oyi uchun {invoices.Count} ta kafega oylik invoyslar chiqarildi (balansida yetarli mablag'i bo'lgan kafelardan avtomatik to'lov yechildi).";

            return RedirectToAction(nameof(Invoices));
        }

        // Kafe balansini to'ldirish (To'lov qabul qilinganda)
        // Balans to'ldirilgach, avtomatik pul yechiladi, muddat 1 oyga uzaytiriladi va ortiqchasi balansda qoladi!
        [HttpPost("owners/topup")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopUpBalance(int ownerId, decimal amount, string? note = null)
        {
            if (amount <= 0)
            {
                TempData["ErrorMessage"] = "To'ldirish summasi 0 dan katta bo'lishi kerak.";
                return RedirectToAction(nameof(OwnerDetail), new { id = ownerId });
            }

            var success = await _billingService.TopUpBalanceAndProcessPaymentAsync(ownerId, amount, note);
            if (success)
            {
                TempData["SuccessMessage"] = $"Balans {amount:N0} so'mga to'ldirildi. Tizim avtomatik to'lovni yechdi va ortiqcha mablag' balansda saqlandi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Balansni to'ldirishda xatolik yuz berdi. Balki foydalanuvchi topilmagan bo'lishi mumkin.";
            }

            return RedirectToAction(nameof(OwnerDetail), new { id = ownerId });
        }

        // Invoysni balansdan to'lash
        [HttpPost("invoices/pay")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PayInvoice(int invoiceId)
        {
            var success = await _billingService.ProcessInvoicePaymentAsync(invoiceId);
            if (success)
            {
                TempData["SuccessMessage"] = "Invoys bo'yicha to'lov balansdan yechildi va kafening faollik muddati 1 oyga uzaytirildi!";
            }
            else
            {
                TempData["ErrorMessage"] = "To'lov amalga oshmadi. Kafe balansida mablag' yetarli emas yoki invoys allaqachon to'langan.";
            }

            return RedirectToAction(nameof(Invoices));
        }

        // Kafeni vaqtincha to'xtatish yoki qayta yoqish
        [HttpPost("cafes/toggle-suspend")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSuspend(int cafeId)
        {
            var success = await _billingService.ToggleCafeSuspendStatusAsync(cafeId);
            if (success)
            {
                TempData["SuccessMessage"] = "Kafening ishlash holati muvaffaqiyatli o'zgartirildi.";
            }

            return RedirectToAction(nameof(Cafes));
        }

        // Faollik muddatini qo'lda uzaytirish
        [HttpPost("cafes/extend")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExtendActiveUntil(int cafeId, int months = 1)
        {
            var success = await _billingService.ExtendActiveUntilManuallyAsync(cafeId, months);
            if (success)
            {
                TempData["SuccessMessage"] = $"Kafening faollik muddati {months} oyga uzaytirildi.";
            }

            return RedirectToAction(nameof(Cafes));
        }

        // ==========================================
        // SESSIONS
        // ==========================================

        // Faol sessiyalar monitoringi
        [HttpGet("sessions")]
        public async Task<IActionResult> Sessions()
        {
            var sessions = await _authService.GetAllActiveSessionsAsync();
            return View(sessions);
        }

        // Sessiyani majburiy bekor qilish (Revoke)
        [HttpPost("revoke-session")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeSession(string sessionId)
        {
            await _authService.RevokeSessionAsync(sessionId);
            TempData["SuccessMessage"] = "Sessiya majburiy to'xtatildi.";
            return RedirectToAction(nameof(Sessions));
        }

        // ==========================================
        // SETTINGS
        // ==========================================

        [HttpGet("settings")]
        public async Task<IActionResult> Settings([FromServices] AppDbContext context)
        {
            var settings = await context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new SystemSettings();
                context.SystemSettings.Add(settings);
                await context.SaveChangesAsync();
            }
            return View(settings);
        }

        [HttpPost("settings/update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSettings(decimal basePrice, int baseUsersLimit, decimal pricePerExtraUsers, int extraUsersStep, [FromServices] AppDbContext context)
        {
            var settings = await context.SystemSettings.FirstOrDefaultAsync();
            if (settings != null)
            {
                settings.BasePrice = basePrice;
                settings.BaseUsersLimit = baseUsersLimit;
                settings.PricePerExtraUsers = pricePerExtraUsers;
                settings.ExtraUsersStep = extraUsersStep;
                settings.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Sozlamalar muvaffaqiyatli saqlandi!";
            }
            return RedirectToAction(nameof(Settings));
        }
    }
}
