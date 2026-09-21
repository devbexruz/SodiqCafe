using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SodiqCafeMVC.Domain.Entities;
using SodiqCafeMVC.Infrastructure.Data;
using SodiqCafeMVC.Domain.Enums;

namespace SodiqCafeMVC.Web.Controllers
{
    [Route("menyu")]
    public class MenyuController : Controller
    {
        private readonly ILogger<MenyuController> _logger;
        private readonly AppDbContext _context;

        public MenyuController(ILogger<MenyuController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var dbCafes = await _context.Cafes
                .Where(c => !c.IsSuspended && DateTime.UtcNow <= c.ActiveUntil)
                .ToListAsync();
            return View(dbCafes);
        }

        [HttpGet("kafe/{cafeId:int}")]
        public async Task<IActionResult> CafeMenu(int cafeId, [FromQuery] string format)
        {
            var dbCafe = await _context.Cafes.FirstOrDefaultAsync(c => c.Id == cafeId);

            if (dbCafe != null)
            {
                if (!dbCafe.IsActive)
                {
                    return View("CafeSuspended", dbCafe);
                }
            }
            else
            {
                return NotFound();
            }

            var items = await _context.Products.Where(p => p.CafeId == cafeId).ToListAsync();

            var visibleCampaigns = await _context.BonusCampaigns
                .Where(bc => bc.CafeId == cafeId && bc.IsActive && bc.IsVisible)
                .ToListAsync();

            ViewBag.CafeId = dbCafe.Id;
            ViewBag.CafeName = dbCafe.Name;
            ViewBag.CafeLogoUrl = dbCafe.LogoUrl;
            ViewBag.IsFullFormat = format == "full";
            ViewBag.VisibleCampaigns = visibleCampaigns;

            return View(items);
        }

        [HttpGet("item/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (item == null) return NotFound();

            return View(item);
        }
    }
}