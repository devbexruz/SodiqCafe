using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SodiqCafeMVC.Application.Interfaces;

namespace SodiqCafeMVC.Infrastructure.Services
{
    public class SetTelegramBotWebhookService : ISetTelegramBotWebhook
    {
        private readonly ILogger<SetTelegramBotWebhookService> _logger;
        public SetTelegramBotWebhookService(ILogger<SetTelegramBotWebhookService> logger)
        {
            _logger = logger;
        }

        public async Task SetWebhookAsync(string webhookUrl, string botToken, string? secretToken = null)
        {
            var setWebhookApiUrl = $"https://api.telegram.org/bot{botToken}/setWebhook?url={Uri.EscapeDataString(webhookUrl)}";
            
            if (!string.IsNullOrEmpty(secretToken))
            {
                setWebhookApiUrl += $"&secret_token={secretToken}";
            }

            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(setWebhookApiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Webhook or'natishda xatolik yuz berdi: {content}");
                    throw new Exception($"Webhook failed. Telegram Response: {content}");
                }
                _logger.LogInformation("Webhook muvaffaqiyatli o'rnatildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telegram Webhook o'rnatishda istisno (exception) yuz berdi.");
                throw;
            }
        }
    }
}
