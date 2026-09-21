using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SodiqCafeMVC.Domain.Entities;
using SodiqCafeMVC.Infrastructure.Data;
using System.Text.Json;

namespace SodiqCafeMVC.Web.ApiControllers
{
    [ApiController]
    [Route("api/queue-admin")]
    public class QueueAdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QueueAdminController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("clear/{cafeId}")]
        public async Task<IActionResult> ClearQueue(int cafeId)
        {
            var today = DateTime.UtcNow.Date;
            
            // Check if snapshot already exists for today to avoid duplicates
            var existingSnapshot = await _context.DailySnapshots.FirstOrDefaultAsync(s => s.CafeId == cafeId && s.Date == today);
            
            if (existingSnapshot == null)
            {
                // Hozirgi kunda nechta check (request) bo'lganini sanaymiz
                var totalChecks = await _context.ServiceRequests.CountAsync(sr => sr.CafeId == cafeId && sr.CreatedAt >= today);
                
                // Hozirgi kunda qancha bonus berilgani
                var todayRewards = await _context.UserRewards
                    .Include(r => r.BonusCampaign)
                    .Where(r => r.CafeId == cafeId && r.EarnedAt >= today)
                    .GroupBy(r => r.BonusCampaign.Name)
                    .Select(g => new { BonusName = g.Key, Count = g.Count() })
                    .ToListAsync();

                var snapshot = new DailySnapshot
                {
                    CafeId = cafeId,
                    Date = today,
                    TotalChecks = totalChecks,
                    TotalBonusesGiven = todayRewards.Sum(r => r.Count),
                    BonusesJson = JsonSerializer.Serialize(todayRewards),
                    CreatedAt = DateTime.UtcNow
                };

                _context.DailySnapshots.Add(snapshot);
            }

            // Endi ServiceRequest jadvallari (navbatlar) tozalanadi
            var requestsToDelete = await _context.ServiceRequests.Where(sr => sr.CafeId == cafeId).ToListAsync();
            _context.ServiceRequests.RemoveRange(requestsToDelete);
            
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
