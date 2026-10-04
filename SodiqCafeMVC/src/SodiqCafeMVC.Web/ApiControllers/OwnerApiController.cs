using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SodiqCafeMVC.Domain.Enums;
using SodiqCafeMVC.Infrastructure.Data;
using SodiqCafeMVC.Domain.Entities;

namespace SodiqCafeMVC.Web.ApiControllers;

[Route("api/v1/owner")]
[Authorize(Roles = "CafeOwner")]
public class OwnerApiController : BaseApiController
{
    private readonly AppDbContext _context;

    public OwnerApiController(AppDbContext context)
    {
        _context = context;
    }

    private int GetOwnerId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("cafes")]
    public async Task<IActionResult> Cafes()
    {
        var ownerId = GetOwnerId();
        var cafes = await _context.Cafes
            .Where(c => c.OwnerId == ownerId)
            .Select(c => new { c.Id, c.Name, c.ActiveUntil, c.IsSuspended })
            .ToListAsync();
        return ApiOk(cafes);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard([FromQuery] int cafeId)
    {
        var ownerId = GetOwnerId();
        var cafe = await _context.Cafes
            .Include(c => c.Owner)
            .FirstOrDefaultAsync(c => c.Id == cafeId && c.OwnerId == ownerId);
        if (cafe == null) return ApiFail("Kafe topilmadi", 404);

        var today = DateTime.UtcNow.Date;
        var todayRequests = await _context.ServiceRequests
            .Where(r => r.CafeId == cafeId && r.CreatedAt >= today && r.Status == ServiceRequestStatus.Completed)
            .CountAsync();
            
        var totalCustomers = await _context.CafeUsers.CountAsync(c => c.CafeId == cafeId);
        var totalProducts = await _context.Products.CountAsync(p => p.CafeId == cafeId && !p.IsDefaultTemplate);
        var daysLeft = (cafe.ActiveUntil - DateTime.UtcNow).Days;
        var balance = cafe.Owner?.Balance ?? 0;
        var basePrice = cafe.BasePrice;
        var totalBonusesGiven = 0; // MVC da ham 0 qilingan

        return ApiOk(new { 
            Cafe = new { cafe.Id, cafe.Name }, 
            TodayServed = todayRequests, 
            TotalCustomers = totalCustomers,
            TotalProducts = totalProducts,
            DaysLeft = daysLeft,
            Balance = balance,
            BasePrice = basePrice,
            TotalBonusesGiven = totalBonusesGiven
        });
    }

    [HttpGet("products")]
    public async Task<IActionResult> Products([FromQuery] int cafeId)
    {
        var ownerId = GetOwnerId();
        var exists = await _context.Cafes.AnyAsync(c => c.Id == cafeId && c.OwnerId == ownerId);
        if (!exists) return ApiFail("Ruxsat yo'q", 403);

        var products = await _context.Products
            .Where(p => p.CafeId == cafeId)
            .ToListAsync();
        return ApiOk(products);
    }

    [HttpGet("products/templates")]
    public async Task<IActionResult> ProductTemplates()
    {
        var templates = await _context.Products
            .Where(p => p.IsDefaultTemplate)
            .ToListAsync();
        return ApiOk(templates);
    }

    [HttpPost("products/add-from-template")]
    public async Task<IActionResult> AddProductFromTemplate([FromBody] AddFromTemplateRequest req)
    {
        var ownerId = GetOwnerId();
        var exists = await _context.Cafes.AnyAsync(c => c.Id == req.CafeId && c.OwnerId == ownerId);
        if (!exists) return ApiFail("Ruxsat yo'q", 403);

        var template = await _context.Products.FirstOrDefaultAsync(p => p.Id == req.TemplateId && p.IsDefaultTemplate);
        if (template == null) return ApiFail("Shablon topilmadi", 404);

        var newProduct = new Product
        {
            Name = template.Name,
            Description = template.Description,
            ImageUrl = template.ImageUrl,
            Category = template.Category,
            Price = req.CustomPrice > 0 ? req.CustomPrice : template.Price,
            IsDefaultTemplate = false,
            CafeId = req.CafeId,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Products.AddAsync(newProduct);
        await _context.SaveChangesAsync();

        return ApiOk(newProduct);
    }

    [HttpPost("products/add")]
    public async Task<IActionResult> AddProduct([FromBody] AddCustomProductRequest req)
    {
        var ownerId = GetOwnerId();
        var exists = await _context.Cafes.AnyAsync(c => c.Id == req.CafeId && c.OwnerId == ownerId);
        if (!exists) return ApiFail("Ruxsat yo'q", 403);

        var product = new Product
        {
            Name = req.Name,
            Price = req.Price,
            Description = req.Description,
            IsRecommended = req.IsRecommended,
            Category = req.Category,
            IsDefaultTemplate = false,
            CafeId = req.CafeId,
            CreatedAt = DateTime.UtcNow,
            ImageUrl = "/img/default-product.png" // Default image for MVP since mobile does not upload yet
        };

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return ApiOk(product);
    }

    [HttpPost("products/edit")]
    public async Task<IActionResult> EditProduct([FromBody] EditProductRequest req)
    {
        var ownerId = GetOwnerId();
        var product = await _context.Products
            .Include(p => p.Cafe)
            .FirstOrDefaultAsync(p => p.Id == req.ProductId && p.Cafe.OwnerId == ownerId);

        if (product == null) return ApiFail("Mahsulot topilmadi", 404);

        product.Name = req.Name;
        product.Price = req.Price;
        product.Description = req.Description;
        product.IsRecommended = req.IsRecommended;
        product.Category = req.Category;

        await _context.SaveChangesAsync();
        return ApiOk(product);
    }

    [HttpPost("products/delete")]
    public async Task<IActionResult> DeleteProduct([FromBody] DeleteProductRequest req)
    {
        var ownerId = GetOwnerId();
        var product = await _context.Products
            .Include(p => p.Cafe)
            .FirstOrDefaultAsync(p => p.Id == req.ProductId && p.Cafe.OwnerId == ownerId);

        if (product == null) return ApiFail("Mahsulot topilmadi", 404);

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return ApiOk(new { success = true });
    }
    [HttpGet("bonus-campaigns")]
    public async Task<IActionResult> BonusCampaigns([FromQuery] int cafeId)
    {
        var ownerId = GetOwnerId();
        var exists = await _context.Cafes.AnyAsync(c => c.Id == cafeId && c.OwnerId == ownerId);
        if (!exists) return ApiFail("Ruxsat yo'q", 403);

        var campaigns = await _context.BonusCampaigns
            .Where(b => b.CafeId == cafeId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
            
        return ApiOk(campaigns);
    }

    [HttpPost("bonus-campaigns/add")]
    public async Task<IActionResult> AddBonusCampaign([FromBody] BonusCampaign model)
    {
        var ownerId = GetOwnerId();
        var exists = await _context.Cafes.AnyAsync(c => c.Id == model.CafeId && c.OwnerId == ownerId);
        if (!exists) return ApiFail("Ruxsat yo'q", 403);

        model.CreatedAt = DateTime.UtcNow;
        await _context.BonusCampaigns.AddAsync(model);
        await _context.SaveChangesAsync();
        
        return ApiOk(model);
    }

    [HttpPost("bonus-campaigns/toggle")]
    public async Task<IActionResult> ToggleBonusCampaign([FromBody] ToggleBonusRequest req)
    {
        var ownerId = GetOwnerId();
        var campaign = await _context.BonusCampaigns
            .Include(b => b.Cafe)
            .FirstOrDefaultAsync(b => b.Id == req.CampaignId && b.Cafe.OwnerId == ownerId);
            
        if (campaign == null) return ApiFail("Topilmadi yoki ruxsat yo'q", 404);

        campaign.IsActive = !campaign.IsActive;
        await _context.SaveChangesAsync();
        
        return ApiOk(campaign);
    }

    [HttpPost("bonus-campaigns/delete")]
    public async Task<IActionResult> DeleteBonusCampaign([FromBody] DeleteBonusRequest req)
    {
        var ownerId = GetOwnerId();
        var campaign = await _context.BonusCampaigns
            .Include(b => b.Cafe)
            .FirstOrDefaultAsync(b => b.Id == req.CampaignId && b.Cafe.OwnerId == ownerId);
            
        if (campaign == null) return ApiFail("Topilmadi yoki ruxsat yo'q", 404);

        _context.BonusCampaigns.Remove(campaign);
        await _context.SaveChangesAsync();
        
        return ApiOk(new { success = true });
    }
    [HttpGet("history")]
    public async Task<IActionResult> History([FromQuery] int cafeId)
    {
        var ownerId = GetOwnerId();
        var exists = await _context.Cafes.AnyAsync(c => c.Id == cafeId && c.OwnerId == ownerId);
        if (!exists) return ApiFail("Ruxsat yo'q", 403);

        var today = DateTime.UtcNow.Date;
        var history = await _context.ServiceRequests
            .Include(sr => sr.User)
            .Where(sr => sr.CafeId == cafeId && sr.Status == ServiceRequestStatus.Completed && sr.CreatedAt >= today)
            .OrderByDescending(sr => sr.CompletedAt)
            .Select(sr => new {
                sr.Id,
                sr.DailyOrderNumber,
                User = sr.User != null ? new { sr.User.Id, sr.User.FullName, sr.User.PhoneNumber } : null,
                sr.IpAddress,
                sr.CompletedAt,
                sr.UserId
            })
            .ToListAsync();

        var todaysRewards = await _context.UserRewards
            .Include(ur => ur.BonusCampaign)
            .Where(ur => ur.CafeId == cafeId && ur.EarnedAt >= today)
            .Select(ur => new {
                ur.UserId,
                ur.EarnedAt,
                BonusCampaign = new { ur.BonusCampaign.Name, ur.BonusCampaign.RewardDescription }
            })
            .ToListAsync();

        return ApiOk(new { History = history, Rewards = todaysRewards });
    }
}

public class ToggleBonusRequest { public int CampaignId { get; set; } }
public class DeleteBonusRequest { public int CampaignId { get; set; } }

public class AddFromTemplateRequest
{
    public int TemplateId { get; set; }
    public decimal CustomPrice { get; set; }
    public int CafeId { get; set; }
}

public class AddCustomProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsRecommended { get; set; }
    public ProductCategory Category { get; set; }
    public int CafeId { get; set; }
}

public class EditProductRequest
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsRecommended { get; set; }
    public ProductCategory Category { get; set; }
}

public class DeleteProductRequest
{
    public int ProductId { get; set; }
}
