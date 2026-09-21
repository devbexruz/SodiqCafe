using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SodiqCafeMVC.Domain.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using SodiqCafeMVC.Infrastructure.Data;
using SodiqCafeMVC.Application.Interfaces;
using System.Net.Http;

namespace SodiqCafeMVC.Infrastructure.Services
{
    public class CafeTelegramBotService: ICafeTelegramBotService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CafeTelegramBotService> _logger;
        private readonly ICafeTelegramBotClientManager _cafeBotClientManager;
        // DbContext orqali cafe ma'lumotlarini olish va bot tokenlarini olish mumkin
        private readonly AppDbContext _dbContext;

        public CafeTelegramBotService(
            IServiceProvider serviceProvider,
            ILogger<CafeTelegramBotService> logger,
            IAppDbContext dbContext,
            ICafeTelegramBotClientManager cafeTelegramBotClientManager)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _dbContext = (AppDbContext)dbContext;
            _cafeBotClientManager = cafeTelegramBotClientManager;
        }

        public async Task ProcessUpdateAsync(Cafe cafe, Update update)
        {
            // Here you would typically instantiate a TelegramBotClient specific to the cafe,
            // using the cafe's bot token from the database.
            // For now, we'll just log it.
            if (string.IsNullOrEmpty(cafe.TelegramBotToken))
            {
                // If token is not set, log a warning and return
                _logger.LogWarning($"Cafe {cafe.Id} does not have a Telegram bot token.");
                return;
            }
            // Create Cafe Telegram Bot Client
            ITelegramBotClient botClient = _cafeBotClientManager.GetClient(cafe.Id, cafe.TelegramBotToken);
            
            // Handle different types of updates (messages, callback queries, etc.)
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                // Callback query handling
                await HandleCallbackQueryAsync(botClient, update.CallbackQuery, cafe, CancellationToken.None);
            }
            if (update.Type == UpdateType.Message && update.Message != null)
            {
                // Message handling
                await HandleMessageAsync(botClient, update.Message, cafe, CancellationToken.None);
            }
        }

        // Message handling method
        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, Cafe cafe, CancellationToken cancellationToken)
        {
            if (message.Text != null)
            {
                _logger.LogInformation($"Received message for cafe {cafe.Id} from {message.Chat.Id}: {message.Text}");
                if (message.Text.StartsWith("/start bonus"))
                {
                    // /start komandasi uchun javob
                    await botClient.SendMessage(message.Chat.Id, $"{cafe.Name} ga xush kelibsiz!", cancellationToken: cancellationToken);
                }
                else if(message.Text.StartsWith("/start"))
                {
                    // /start komandasi uchun javob
                    await botClient.SendMessage(message.Chat.Id, $"{cafe.Name} ga xush kelibsiz!", cancellationToken: cancellationToken);
                }
            }
        }

        // Callback query handling method
        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, Cafe cafe, CancellationToken cancellationToken)
        {
            var data = callbackQuery.Data;
            if (string.IsNullOrEmpty(data)) return;
            
            _logger.LogInformation($"Received callback for cafe {cafe.Id}: {data}");
        }
    }
}
