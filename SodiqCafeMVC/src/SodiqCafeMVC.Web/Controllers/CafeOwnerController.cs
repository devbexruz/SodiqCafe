using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SodiqCafeMVC.Domain.Entities;
using SodiqCafeMVC.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;

namespace SodiqCafeMVC.Web.Controllers
{
    [Authorize(Roles = "CafeOwner")]
    [Route("cafe-owner")]
    public class CafeOwnerController : Controller
    {
        private readonly AppDbContext _context;

        public CafeOwnerController(AppDbContext context)
        {
            _context = context;
        }

        private int? GetSelectedCafeId()
        {
            if (int.TryParse(Request.Query["cafeId"], out int cafeIdFromQuery))
                return cafeIdFromQuery;

            if (Request.Cookies.TryGetValue("sodiqcafe_selected_cafe_id", out string? cafeIdCookie) && int.TryParse(cafeIdCookie, out int cafeIdFromCookie))
                return cafeIdFromCookie;

            return null;
        }

        [HttpGet("select-cafe/{id}")]
        public async Task<IActionResult> SelectCafe(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var cafe = await _context.Cafes.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);
            if (cafe != null)
            {
                Response.Cookies.Append("sodiqcafe_selected_cafe_id", cafe.Id.ToString(), new CookieOptions { Expires = DateTime.UtcNow.AddDays(30) });
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet("")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return View("NoCafes");

            ViewBag.SelectedCafeId = cafe.Id;
            
            // Dashboard statistika
            ViewBag.TotalProducts = await _context.Products.CountAsync(p => p.CafeId == cafe.Id && !p.IsDefaultTemplate);
            ViewBag.TotalCustomers = await _context.CafeUsers.CountAsync(cu => cu.CafeId == cafe.Id);
            ViewBag.TotalBonusesGiven = 0;

            return View(cafe);
        }

        [HttpGet("products")]
        public async Task<IActionResult> Products()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return View("NoCafes");

            ViewBag.SelectedCafeId = cafe.Id;
            ViewBag.CafeName = cafe.Name;
            
            var products = await _context.Products
                .Where(p => p.CafeId == cafe.Id && !p.IsDefaultTemplate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var defaultTemplates = await _context.Products
                .Where(p => p.IsDefaultTemplate)
                .ToListAsync();

            ViewBag.DefaultTemplates = defaultTemplates;

            return View(products);
        }

        [HttpPost("products/add-from-template")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductFromTemplate(int templateId, decimal customPrice)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return RedirectToAction(nameof(Dashboard));

            var template = await _context.Products.FirstOrDefaultAsync(p => p.Id == templateId && p.IsDefaultTemplate);
            if (template != null)
            {
                var newProduct = new Product
                {
                    Name = template.Name,
                    Description = template.Description,
                    ImageUrl = template.ImageUrl,
                    Category = template.Category,
                    Price = customPrice > 0 ? customPrice : template.Price,
                    IsDefaultTemplate = false,
                    CafeId = cafe.Id,
                    CreatedAt = DateTime.UtcNow
                };
                
                await _context.Products.AddAsync(newProduct);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"{newProduct.Name} menyuga muvaffaqiyatli qo'shildi.";
            }

            return RedirectToAction(nameof(Products));
        }

        [HttpPost("products/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return RedirectToAction(nameof(Dashboard));

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.CafeId == cafe.Id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mahsulot o'chirildi.";
            }

            return RedirectToAction(nameof(Products));
        }

        [HttpPost("products/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(string name, decimal price, string? description, bool isRecommended, SodiqCafeMVC.Domain.Enums.ProductCategory category, Microsoft.AspNetCore.Http.IFormFile? imageFile, [FromServices] SodiqCafeMVC.Application.Interfaces.IFileStorageService fileStorageService)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return RedirectToAction(nameof(Dashboard));

            string? imageUrl = null;
            if (imageFile != null && imageFile.Length > 0)
            {
                using var stream = imageFile.OpenReadStream();
                imageUrl = await fileStorageService.UploadFileAsync(stream, imageFile.FileName, imageFile.ContentType);
            }

            var newProduct = new Product
            {
                Name = name,
                Price = price,
                Description = description,
                IsRecommended = isRecommended,
                Category = category,
                ImageUrl = imageUrl,
                IsDefaultTemplate = false,
                CafeId = cafe.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Products.AddAsync(newProduct);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Mahsulot muvaffaqiyatli qo'shildi.";

            return RedirectToAction(nameof(Products));
        }

        [HttpPost("products/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int productId, string name, decimal price, string? description, bool isRecommended, SodiqCafeMVC.Domain.Enums.ProductCategory category, Microsoft.AspNetCore.Http.IFormFile? imageFile, [FromServices] SodiqCafeMVC.Application.Interfaces.IFileStorageService fileStorageService)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return RedirectToAction(nameof(Dashboard));

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId && p.CafeId == cafe.Id);
            if (product != null)
            {
                product.Name = name;
                product.Price = price;
                product.Description = description;
                product.IsRecommended = isRecommended;
                product.Category = category;

                if (imageFile != null && imageFile.Length > 0)
                {
                    using var stream = imageFile.OpenReadStream();
                    product.ImageUrl = await fileStorageService.UploadFileAsync(stream, imageFile.FileName, imageFile.ContentType);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Mahsulot yangilandi.";
            }

            return RedirectToAction(nameof(Products));
        }

        [HttpGet("bonus-campaigns")]
        public async Task<IActionResult> BonusCampaigns()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return View("NoCafes");

            ViewBag.SelectedCafeId = cafe.Id;
            ViewBag.CafeName = cafe.Name;
            
            var campaigns = await _context.BonusCampaigns
                .Where(c => c.CafeId == cafe.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(campaigns);
        }

        [HttpPost("bonus-campaigns/add")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBonusCampaign(string name, string description, SodiqCafeMVC.Domain.Enums.BonusType type, decimal conditionValue, string rewardDescription, bool isVisible)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return RedirectToAction(nameof(Dashboard));

            var campaign = new BonusCampaign
            {
                CafeId = cafe.Id,
                Name = name,
                Description = description,
                Type = type,
                ConditionValue = conditionValue,
                RewardDescription = rewardDescription,
                IsVisible = isVisible,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.BonusCampaigns.Add(campaign);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yangi bonus e'loni muvaffaqiyatli qo'shildi.";
            return RedirectToAction(nameof(BonusCampaigns));
        }

        [HttpPost("bonus-campaigns/toggle-active")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBonusCampaignActive(int campaignId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return RedirectToAction(nameof(Dashboard));

            var campaign = await _context.BonusCampaigns.FirstOrDefaultAsync(c => c.Id == campaignId && c.CafeId == cafe.Id);
            if (campaign != null)
            {
                campaign.IsActive = !campaign.IsActive;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Holat o'zgartirildi.";
            }

            return RedirectToAction(nameof(BonusCampaigns));
        }

        [HttpPost("bonus-campaigns/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBonusCampaign(int campaignId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return RedirectToAction(nameof(Dashboard));

            var campaign = await _context.BonusCampaigns.FirstOrDefaultAsync(c => c.Id == campaignId && c.CafeId == cafe.Id);
            if (campaign != null)
            {
                _context.BonusCampaigns.Remove(campaign);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Aksiya o'chirildi.";
            }

            return RedirectToAction(nameof(BonusCampaigns));
        }

        [HttpGet("invoices")]
        public async Task<IActionResult> Invoices([FromServices] SodiqCafeMVC.Application.Interfaces.IBillingService billingService)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return View("NoCafes");

            ViewBag.SelectedCafeId = cafe.Id;
            ViewBag.CafeName = cafe.Name;

            var invoices = await billingService.GetCafeInvoicesAsync(cafe.Id);
            return View(invoices);
        }

        [HttpPost("invoices/upload-receipt")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadReceipt(int invoiceId, Microsoft.AspNetCore.Http.IFormFile receiptImage, 
            [FromServices] SodiqCafeMVC.Application.Interfaces.ISystemTelegramBotService telegramBotService,
            [FromServices] SodiqCafeMVC.Application.Interfaces.IFileStorageService fileStorageService)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var invoice = await _context.Invoices
                .Include(i => i.Cafe)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && i.Cafe.OwnerId == userId);

            if (invoice == null) return NotFound();

            if (receiptImage != null && receiptImage.Length > 0)
            {
                using var stream = receiptImage.OpenReadStream();
                invoice.ReceiptImageUrl = await fileStorageService.UploadFileAsync(stream, receiptImage.FileName, receiptImage.ContentType);
                invoice.Status = SodiqCafeMVC.Domain.Enums.InvoiceStatus.Pending; // Or WaitingForApproval
                
                await _context.SaveChangesAsync();
                
                // Send to Telegram Admin Bot for approval
                await telegramBotService.SendInvoiceForApprovalAsync(
                    invoice.Id, 
                    invoice.Cafe.Name, 
                    invoice.Amount, 
                    invoice.ReceiptImageUrl, 
                    invoice.InvoiceNumber);

                TempData["SuccessMessage"] = "Kvitansiya yuklandi va adminga tasdiqlash uchun yuborildi. Tez orada ko'rib chiqiladi.";
            }

            return RedirectToAction(nameof(Invoices));
        }
        [HttpGet("queue")]
        public async Task<IActionResult> Queue()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return View("NoCafes");

            ViewBag.SelectedCafeId = cafe.Id;
            ViewBag.CafeName = cafe.Name;
            
            var today = DateTime.UtcNow.Date;
            var requests = await _context.ServiceRequests
                .Include(r => r.User)
                .Where(r => r.CafeId == cafe.Id && r.CreatedAt >= today && r.Status != SodiqCafeMVC.Domain.Enums.ServiceRequestStatus.Completed)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            return View(requests);
        }

        [HttpGet("serve-customer/{requestId}")]
        public async Task<IActionResult> ServeCustomer(int requestId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var request = await _context.ServiceRequests
                .Include(r => r.User)
                .Include(r => r.Cafe)
                .FirstOrDefaultAsync(r => r.Id == requestId && r.Cafe.OwnerId == userId);
            
            if (request == null) return NotFound();

            // Statusni xizmat ko'rsatilmoqda deb o'zgartiramiz
            if (request.Status == SodiqCafeMVC.Domain.Enums.ServiceRequestStatus.Waiting)
            {
                request.Status = SodiqCafeMVC.Domain.Enums.ServiceRequestStatus.Serving;
                await _context.SaveChangesAsync();
                
                // SignalR orqali mijozga xabar beramiz
                var hubContext = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.SignalR.IHubContext<SodiqCafeMVC.Web.Hubs.NfcHub>)) as Microsoft.AspNetCore.SignalR.IHubContext<SodiqCafeMVC.Web.Hubs.NfcHub>;
                if (hubContext != null)
                {
                    await hubContext.Clients.Group($"Order_{request.Id}").SendAsync("CustomerTurn", request.Id);
                }
            }

            bool isNewCustomer = true;
            double daysSinceLastVisit = 0;
            List<UserCampaignProgress> userProgresses = new List<UserCampaignProgress>();

            if (request.UserId.HasValue)
            {
                var cafeUser = await _context.CafeUsers
                    .FirstOrDefaultAsync(cu => cu.UserId == request.UserId.Value && cu.CafeId == request.CafeId);
                
                isNewCustomer = cafeUser == null;
                daysSinceLastVisit = cafeUser != null ? (DateTime.UtcNow - cafeUser.LastActivityAt).TotalDays : 0;

                userProgresses = await _context.UserCampaignProgresses
                    .Where(p => p.UserId == request.UserId.Value && p.BonusCampaign.CafeId == request.CafeId)
                    .ToListAsync();
            }

            // Barcha faol aksiyalar
            var activeCampaigns = await _context.BonusCampaigns
                .Where(c => c.CafeId == request.CafeId && c.IsActive)
                .ToListAsync();

            ViewBag.Request = request;
            ViewBag.IsNewCustomer = isNewCustomer;
            ViewBag.DaysSinceLastVisit = daysSinceLastVisit;
            ViewBag.UserProgresses = userProgresses;

            return View(activeCampaigns);
        }

        [HttpPost("issue-bonus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IssueBonus(int requestId, int[] selectedCampaignIds, decimal totalAmount)
        {
            var request = await _context.ServiceRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);
            
            if (request == null) return NotFound();

            if (request.UserId.HasValue)
            {
                var cafeUser = await _context.CafeUsers
                    .FirstOrDefaultAsync(cu => cu.UserId == request.UserId.Value && cu.CafeId == request.CafeId);
                
                if (cafeUser == null)
                {
                    cafeUser = new CafeUser { UserId = request.UserId.Value, CafeId = request.CafeId };
                    _context.CafeUsers.Add(cafeUser);
                }
                
                cafeUser.LastActivityAt = DateTime.UtcNow;
            }

            var campaignsToProcess = await _context.BonusCampaigns
                .Where(c => selectedCampaignIds.Contains(c.Id) && c.CafeId == request.CafeId && c.IsActive)
                .ToListAsync();

            int rewardsGiven = 0;
            var newTokens = new System.Collections.Generic.List<string>();

            // Bonus berish logikasi (userId bor yoki yo'q bo'lishidan qat'iy nazar)
            foreach (var campaign in campaignsToProcess)
            {
                bool giveReward = false;
                if (campaign.Type == SodiqCafeMVC.Domain.Enums.BonusType.StampCard)
                {
                    if (request.UserId.HasValue)
                    {
                        var progress = await _context.UserCampaignProgresses.FirstOrDefaultAsync(p => p.UserId == request.UserId.Value && p.BonusCampaignId == campaign.Id);
                        if (progress == null)
                        {
                            progress = new UserCampaignProgress { UserId = request.UserId.Value, BonusCampaignId = campaign.Id, CurrentValue = 0 };
                            _context.UserCampaignProgresses.Add(progress);
                        }
                        
                        progress.CurrentValue += 1;
                        progress.LastUpdatedAt = DateTime.UtcNow;

                        if (progress.CurrentValue >= campaign.ConditionValue)
                        {
                            giveReward = true;
                            progress.CurrentValue = 0;
                        }
                    }
                    else
                    {
                        // Anonim mijoz uchun stamp card ishlashi? Hozircha darhol bitta stamp deb hisoblaymiz yoki shunchaki beramiz.
                        // Yoki anonim bo'lsa darhol reward yaratamiz, lekin mijoz botda ro'yxatdan o'tgach, progress hisoblanishi mumkin.
                        // Hozircha anonim mijozlarga stamp progress ishlamaydi, shuning uchun darhol yutuq bera qolamiz yoki stampni 1 deb tokenga bog'laymiz.
                        // Lekin UserCampaignProgress da UserId majburiy emas qilish qiyin. Shuning uchun faqat to'g'ridan-to'g'ri beriladigan bonuslarni (masalan N-chi mijoz) beramiz yoki stampni ham darhol reward sifatida yuboramiz.
                        // Keling, talabga ko'ra barcha tanlangan bonuslarni beramiz.
                        giveReward = true;
                    }
                }
                else
                {
                    giveReward = true;
                }

                if (giveReward)
                {
                    string transferToken = Guid.NewGuid().ToString("N");
                    _context.UserRewards.Add(new UserReward
                    {
                        UserId = request.UserId,
                        CafeId = request.CafeId,
                        BonusCampaignId = campaign.Id,
                        IsUsed = false,
                        TransferToken = transferToken,
                        EarnedAt = DateTime.UtcNow
                    });
                    newTokens.Add(transferToken);
                    rewardsGiven++;
                }
            }

            request.Status = SodiqCafeMVC.Domain.Enums.ServiceRequestStatus.Completed;
            request.CompletedAt = DateTime.UtcNow;
            
            // Queue tokenni o'chirmaymiz, chunki kunlik tozalashda o'chadi yoki u holda qayta ishlata olmaydi. 
            // Lekin navbat tugatildi (Completed).

            await _context.SaveChangesAsync();

            // SignalR orqali mijozga yutuqlar berilganini e'lon qilish
            var hubContext = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.SignalR.IHubContext<SodiqCafeMVC.Web.Hubs.NfcHub>)) as Microsoft.AspNetCore.SignalR.IHubContext<SodiqCafeMVC.Web.Hubs.NfcHub>;
            if (hubContext != null)
            {
                string message = rewardsGiven > 0 ? $"Sizga {rewardsGiven} ta bonus/mukofot berildi! Profilingizdan tekshirib ko'ring." : "Xaridingiz uchun rahmat!";
                await hubContext.Clients.Group($"Order_{request.Id}").SendAsync("BonusIssued", request.Id, message, newTokens);
            }

            TempData["SuccessMessage"] = "Mijozga muvaffaqiyatli xizmat ko'rsatildi.";
            return RedirectToAction(nameof(Queue));
        }
    
        [HttpGet("history")]
        public async Task<IActionResult> History()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return View("NoCafes");

            var today = DateTime.UtcNow.Date;
            var history = await _context.ServiceRequests
                .Include(sr => sr.User)
                .Where(sr => sr.CafeId == cafe.Id && sr.Status == SodiqCafeMVC.Domain.Enums.ServiceRequestStatus.Completed && sr.CreatedAt >= today)
                .OrderByDescending(sr => sr.CompletedAt)
                .ToListAsync();

            return View(history);
        }

        [HttpGet("customers")]
        public async Task<IActionResult> Customers()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return View("NoCafes");

            var customers = await _context.CafeUsers
                .Include(cu => cu.User)
                .Where(cu => cu.CafeId == cafe.Id)
                .OrderByDescending(cu => cu.LastActivityAt)
                .Select(cu => new
                {
                    User = cu.User,
                    LastActivityAt = cu.LastActivityAt,
                    TotalPurchases = _context.ServiceRequests.Count(sr => sr.UserId == cu.UserId && sr.CafeId == cafe.Id && sr.Status == SodiqCafeMVC.Domain.Enums.ServiceRequestStatus.Completed)
                })
                .ToListAsync();

            ViewBag.Customers = customers;
            return View();
        }

        [HttpGet("settings")]
        public async Task<IActionResult> Settings()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return View("NoCafes");

            return View(cafe);
        }

        [HttpPost("settings/bot")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBotSettings(string botToken, [FromServices] SodiqCafeMVC.Application.Interfaces.ISetTelegramBotWebhook setWebhookService)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

            var selectedCafeId = GetSelectedCafeId();
            var cafe = selectedCafeId.HasValue 
                ? await _context.Cafes.FirstOrDefaultAsync(c => c.Id == selectedCafeId.Value && c.OwnerId == userId)
                : await _context.Cafes.OrderByDescending(c => c.CreatedAt).FirstOrDefaultAsync(c => c.OwnerId == userId);
            
            if (cafe == null) return NotFound();

            if (string.IsNullOrWhiteSpace(botToken))
            {
                // Disable bot
                cafe.TelegramBotToken = null;
                cafe.TelegramBotUsername = null;
                cafe.TelegramWebhookSecretToken = null;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Telegram bot o'chirildi.";
                return RedirectToAction(nameof(Settings));
            }

            try
            {
                using var httpClient = new System.Net.Http.HttpClient();
                
                // 1. Get bot info
                var response = await httpClient.GetAsync($"https://api.telegram.org/bot{botToken}/getMe");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Noto'g'ri Bot Token kiritildi.";
                    return RedirectToAction(nameof(Settings));
                }

                var content = await response.Content.ReadAsStringAsync();
                var json = System.Text.Json.JsonDocument.Parse(content);
                var result = json.RootElement.GetProperty("result");
                var botUsername = result.GetProperty("username").GetString();

                // 2. Set webhook with auto-generated secret token
                var baseUrl = $"https://sodiqcafe.uz";
                var webhookUrl = $"{baseUrl}/api/cafewebhook/{cafe.Id}";
                var generatedSecret = Guid.NewGuid().ToString("N");
                
                try
                {
                    await setWebhookService.SetWebhookAsync(webhookUrl, botToken, generatedSecret);
                    
                    cafe.TelegramBotToken = botToken;
                    cafe.TelegramBotUsername = botUsername;
                    cafe.TelegramWebhookSecretToken = generatedSecret;
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Telegram bot (@{botUsername}) muvaffaqiyatli sozlandi va Webhook ulandi!";
                }
                catch (Exception ex)
                {
                    TempData["WarningMessage"] = $"Bot topildi (@{botUsername}), lekin Webhook o'rnatishda xatolik yuz berdi. {ex.Message}";
                    
                    // Baribir tokenni saqlab qolamiz, lekin webhook ishlamasligi mumkin.
                    cafe.TelegramBotToken = botToken;
                    cafe.TelegramBotUsername = botUsername;
                    await _context.SaveChangesAsync();
                }
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Xatolik yuz berdi: " + ex.Message;
            }

            return RedirectToAction(nameof(Settings));
        }
    }
}
