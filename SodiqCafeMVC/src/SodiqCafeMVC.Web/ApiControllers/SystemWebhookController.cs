using SodiqCafeMVC.Infrastructure.Services;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SodiqCafeMVC.Application.Interfaces;
using Telegram.Bot.Types;

namespace SodiqCafeMVC.Web.ApiControllers
{
    [ApiController]
    [Route("api/systemwebhook")]
    public class SystemWebhookController : ControllerBase
    {
        private readonly SystemTelegramBotService _botService;
        private readonly string _secretToken;
        private readonly IConfiguration _config;

        public SystemWebhookController(SystemTelegramBotService botService, IConfiguration config)
        {
            _botService = botService;
            _secretToken = config["TelegramBot:WebhookSecretToken"] ?? string.Empty;
            _config = config;
        }

        [HttpGet("setup")]
        public async Task<IActionResult> SetupWebhook()
        {
            var botToken = _config["TelegramBot:Token"];
            if (string.IsNullOrEmpty(botToken)) return BadRequest("Bot token is not configured.");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var webhookUrl = $"{baseUrl}/api/systemwebhook";
            
            var setWebhookApiUrl = $"https://api.telegram.org/bot{botToken}/setWebhook?url={System.Uri.EscapeDataString(webhookUrl)}";
            
            if (!string.IsNullOrEmpty(_secretToken))
            {
                setWebhookApiUrl += $"&secret_token={_secretToken}";
            }

            using var httpClient = new System.Net.Http.HttpClient();
            var response = await httpClient.GetAsync(setWebhookApiUrl);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { Message = "Webhook muvaffaqiyatli o'rnatildi!", TelegramResponse = content });
            }

            return BadRequest(new { Message = "Webhook o'rnatishda xatolik yuz berdi.", TelegramResponse = content });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            // Telegram webhook secret tekshirish
            if (!string.IsNullOrEmpty(_secretToken))
            {
                var incomingToken = Request.Headers["X-Telegram-Bot-Api-Secret-Token"].ToString();
                if (string.IsNullOrEmpty(incomingToken) || incomingToken != _secretToken)
                {
                    return Unauthorized("Invalid secret token");
                }
            }

            try
            {
                await _botService.ProcessUpdateAsync(update);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SystemWebhook Error: {ex.Message}");
            }

            return Ok();
        }
    }
}
