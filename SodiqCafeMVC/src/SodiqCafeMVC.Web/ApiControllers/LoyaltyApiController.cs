using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SodiqCafeMVC.Infrastructure.Data;
using SodiqCafeMVC.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace SodiqCafeMVC.Web.ApiControllers
{
    [Route("api/loyalty")]
    [ApiController]
    [Authorize] // Kassir/CafeOwner uchun cheklov
    public class LoyaltyApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LoyaltyApiController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-stamp")]
        public async Task<IActionResult> AddStamp([FromBody] LoyaltyActionDto dto)
        {
            return Ok(new { success = true, currentStamps = 0 });
        }

        [HttpPost("add-event-bonus")]
        public async Task<IActionResult> AddEventBonus([FromBody] AddEventBonusDto dto)
        {
            return Ok(new { success = true, message = "Maxsus bonus taqdim etildi." });
        }
    }

    public class LoyaltyActionDto
    {
        public int UserId { get; set; }
        public int CafeId { get; set; }
    }

    public class AddEventBonusDto
    {
        public int UserId { get; set; }
        public int CafeId { get; set; }
        public string BonusType { get; set; } = string.Empty;
    }
}
