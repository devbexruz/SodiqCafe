using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SodiqCafeMVC.Domain.Entities;
using SodiqCafeMVC.Domain.Enums;
using SodiqCafeMVC.Infrastructure.Data;
using SodiqCafeMVC.Infrastructure.Services;
using System.Text.Json;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace SodiqCafeMVC.Web.ApiControllers
{
    [ApiController]
    [Route("api/cafewebhook")]
    public class CafeWebhookController : ControllerBase
    {
        private readonly CafeTelegramBotService _botService;
        private readonly AppDbContext _context;

        public CafeWebhookController(CafeTelegramBotService botService, AppDbContext context)
        {
            _botService = botService;
            _context = context;
        }

        [HttpPost("{cafeId}")]
        public async Task<IActionResult> Post(int cafeId, [FromBody] Update update)
        {
            // Cafe ni bazadan topamiz
            var cafe = await _context.Cafes.FirstOrDefaultAsync(c => c.Id == cafeId);
            if (cafe == null) return NotFound("Cafe not found");

            // Telegram webhook secret tekshirish
            var expectedSecret = cafe.TelegramWebhookSecretToken;
            if (!string.IsNullOrEmpty(expectedSecret))
            {
                var incomingToken = Request.Headers["X-Telegram-Bot-Api-Secret-Token"].ToString();
                if (string.IsNullOrEmpty(incomingToken) || incomingToken != expectedSecret)
                {
                    return Unauthorized("Invalid secret token");
                }
            }

            try
            {
                if (string.IsNullOrEmpty(cafe.TelegramBotToken))
                {
                    return BadRequest("Cafe does not have a Telegram bot token.");
                }
                ITelegramBotClient botClient = new TelegramBotClient(cafe.TelegramBotToken);
                await _botService.ProcessUpdateAsync(cafe, update);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CafeWebhook Error: {ex.Message}");
            }

            return Ok();
        }
    }
}
