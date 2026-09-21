using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SodiqCafeMVC.Domain.Entities;
using SodiqCafeMVC.Domain.Enums;
using SodiqCafeMVC.Infrastructure.Data;
using SodiqCafeMVC.Web.Hubs;
using System.Collections.Generic;

namespace SodiqCafeMVC.Web.Controllers
{
    public class BonusProcessController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NfcHub> _hubContext;

        public BonusProcessController(AppDbContext context, IHubContext<NfcHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet("bonus/{cafeId}")]
        public async Task<IActionResult> Index(int cafeId)
        {
            var cafe = await _context.Cafes.FindAsync(cafeId);
            if (cafe == null) return NotFound("Kafe topilmadi.");

            ViewBag.CafeId = cafeId;
            ViewBag.CafeName = cafe.Name;
            ViewBag.CafeLogoUrl = cafe.LogoUrl;
            ViewBag.BotUsername = cafe.TelegramBotUsername;
            
            return View();
        }

        [HttpPost("api/queue/create/{cafeId}")]
        public async Task<IActionResult> CreateQueue(int cafeId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? userId = null;
            if (int.TryParse(userIdStr, out var parsedId))
            {
                userId = parsedId;
            }

            var today = DateTime.UtcNow.Date;
            var maxOrderNumber = await _context.ServiceRequests
                .Where(sr => sr.CafeId == cafeId && sr.CreatedAt >= today)
                .MaxAsync(sr => (int?)sr.DailyOrderNumber) ?? 0;

            var activeRequest = new ServiceRequest
            {
                CafeId = cafeId,
                UserId = userId,
                DailyOrderNumber = maxOrderNumber + 1,
                Status = ServiceRequestStatus.Waiting,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Token = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceRequests.Add(activeRequest);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group($"Cafe_{cafeId}").SendAsync("NewCustomerArrived", new { 
                requestId = activeRequest.Id, 
                orderNumber = activeRequest.DailyOrderNumber,
                status = "Kutmoqda"
            });

            return Ok(new { id = activeRequest.Id, token = activeRequest.Token, dailyOrderNumber = activeRequest.DailyOrderNumber });
        }
        
        [HttpPost("api/queue/activate/{id}")]
        public async Task<IActionResult> ActivateQueue(int id, [FromQuery] string token)
        {
            var request = await _context.ServiceRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (request == null || request.Token != token) return Unauthorized();
            
            // Re-notify seller
            await _hubContext.Clients.Group($"Cafe_{request.CafeId}").SendAsync("NewCustomerArrived", new { 
                requestId = request.Id, 
                orderNumber = request.DailyOrderNumber,
                status = "Kutmoqda"
            });
            
            return Ok(new { success = true });
        }

        [HttpPost("api/bonuses/my")]
        public async Task<IActionResult> GetMyBonuses([FromBody] List<string> tokens)
        {
            if (tokens == null || !tokens.Any()) return Ok(new List<object>());

            var rewards = await _context.UserRewards
                .Include(r => r.BonusCampaign)
                .Include(r => r.Cafe)
                .Where(r => tokens.Contains(r.TransferToken ?? string.Empty))
                .OrderByDescending(r => r.EarnedAt)
                .Select(r => new {
                    r.Id,
                    r.TransferToken,
                    r.IsUsed,
                    r.IsTransferredToBot,
                    r.EarnedAt,
                    CampaignName = r.BonusCampaign.Name,
                    CampaignReward = r.BonusCampaign.RewardDescription,
                    CafeName = r.Cafe.Name
                })
                .ToListAsync();

            return Ok(rewards);
        }
    }
}
