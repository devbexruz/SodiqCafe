using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SodiqCafeMVC.Application.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SodiqCafeMVC.Infrastructure.Services
{
    public class SystemTelegramBotService : ISystemTelegramBotService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SystemTelegramBotService> _logger;
        private readonly string _adminChatId;

        public SystemTelegramBotService(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<SystemTelegramBotService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            
            var token = configuration["TelegramBot:Token"];
            _adminChatId = configuration["TelegramBot:AdminChatId"] ?? string.Empty;

            if (!string.IsNullOrEmpty(token))
            {
                _botClient = new TelegramBotClient(token);
            }
            else
            {
                _botClient = null!;
                _logger.LogWarning("Telegram Bot Token is not configured.");
            }
        }
        public async Task ProcessUpdateAsync(Update update)
        {
            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                await HandleCallbackQueryAsync(_botClient, update.CallbackQuery, CancellationToken.None);
            }
            if (update.Type == UpdateType.Message && update.Message != null)
            {
                await HandleMessageAsync(_botClient, update.Message, CancellationToken.None);
            }
        }

        // Holatlarni saqlash uchun oddiy xotira (Production'da bazaga yoki Redis'ga saqlash tavsiya etiladi)
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<long, string> _userStates = new();

        private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text != null)
            {
                var text = message.Text;
                var chatId = message.Chat.Id;
                
                _logger.LogInformation($"Received message from {chatId}: {text}");

                // 1. Foydalanuvchi qandaydir holatda (state) ekanligini tekshiramiz
                if (_userStates.TryGetValue(chatId, out var state))
                {
                    await HandleStateAsync(botClient, message, state, cancellationToken);
                    return;
                }

                // 2. Buyruqlar (Command) tekshiruvi (masalan: /start, /help)
                if (text.StartsWith("/"))
                {
                    await HandleCommandAsync(botClient, message, cancellationToken);
                    return;
                }

                // 3. Oddiy matnli xabarlar tekshiruvi
                await HandleTextAsync(botClient, message, cancellationToken);
            }
        }

        private async Task HandleCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var command = message.Text!.Split(' ')[0].ToLower();
            var chatId = message.Chat.Id;
            
            switch (command)
            {
                case "/start":
                    string welcomeText = "👋 <b>SodiqCafe tizimiga xush kelibsiz!</b>\n\n" +
                                         "Bizning tizim orqali siz o'z kafengizni to'liq avtomatlashtirish, menyular yaratish va mijozlarga zamonaviy xizmat ko'rsatish imkoniyatiga egasiz.\n\n" +
                                         "Quyidagi buyruqlardan foydalanishingiz mumkin:\n" +
                                         "• /help - Yordam bo'limi\n" +
                                         "• /feedback - Bizga o'z fikringizni yozib qoldiring";
                    
                    await botClient.SendMessage(chatId, welcomeText, parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                    break;
                    
                case "/help":
                    await botClient.SendMessage(chatId, "Yordam kerak bo'lsa, adminga murojaat qiling yoki platformamizga tashrif buyuring.", cancellationToken: cancellationToken);
                    break;

                case "/feedback":
                    // Foydalanuvchini holatini o'zgartiramiz, u endi matn kiritishini kutamiz
                    _userStates[chatId] = "WAITING_FOR_FEEDBACK";
                    await botClient.SendMessage(chatId, "✍️ Iltimos, o'z fikr-mulohazangiz yoki shikoyatingizni yozib yuboring:", cancellationToken: cancellationToken);
                    break;

                default:
                    await botClient.SendMessage(chatId, "Noma'lum buyruq. Iltimos, /start ni bosing.", cancellationToken: cancellationToken);
                    break;
            }
        }

        private async Task HandleTextAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var text = message.Text!;
            var chatId = message.Chat.Id;
            
            if (text == "Menyu")
            {
                // Menyu bosilganda qilinadigan ishlar
            }
            else
            {
                await botClient.SendMessage(chatId, "Kechirasiz, men sizni tushunmadim. Buyruqlarni ko'rish uchun /start bosing.", cancellationToken: cancellationToken);
            }
        }

        private async Task HandleStateAsync(ITelegramBotClient botClient, Message message, string state, CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;

            // Agar foydalanuvchi bekor qilishni xohlasa
            if (message.Text == "/cancel")
            {
                _userStates.TryRemove(chatId, out _);
                await botClient.SendMessage(chatId, "Amal bekor qilindi.", cancellationToken: cancellationToken);
                return;
            }

            switch (state)
            {
                case "WAITING_FOR_FEEDBACK":
                    // Bu yerda xabarni bazaga saqlashingiz yoki adminga jo'natishingiz mumkin
                    string feedback = message.Text!;
                    
                    // Adminga xabar berish (ixtiyoriy)
                    if (!string.IsNullOrEmpty(_adminChatId))
                    {
                        await botClient.SendMessage(_adminChatId, $"Yangi fikr-mulohaza keldi:\n\n{feedback}", cancellationToken: cancellationToken);
                    }

                    // Holatdan chiqarish, chunki kutilgan amal bajarildi
                    _userStates.TryRemove(chatId, out _); 
                    await botClient.SendMessage(chatId, "✅ Rahmat! Fikr-mulohazangiz qabul qilindi.", cancellationToken: cancellationToken);
                    break;
                    
                default:
                    _userStates.TryRemove(chatId, out _);
                    await botClient.SendMessage(chatId, "Xatolik yuz berdi, joriy holat bekor qilindi.", cancellationToken: cancellationToken);
                    break;
            }
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            var data = callbackQuery.Data;
            if (string.IsNullOrEmpty(data)) return;

            var parts = data.Split('_');
            if (parts.Length != 3 || parts[0] != "invoice") return;

            var action = parts[1]; // "approve" or "reject"
            if (!int.TryParse(parts[2], out var invoiceId)) return;

            using var scope = _serviceProvider.CreateScope();
            var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();

            try
            {
                if (action == "approve")
                {
                    var success = await billingService.ProcessInvoicePaymentAsync(invoiceId, isExternalPayment: true);
                    if (success)
                    {
                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "Invoys tasdiqlandi va to'lov amalga oshirildi!", cancellationToken: cancellationToken);
                        
                        // Edit message to remove buttons
                        if (callbackQuery.Message != null)
                        {
                            await botClient.EditMessageReplyMarkup(
                                callbackQuery.Message.Chat.Id,
                                callbackQuery.Message.MessageId,
                                replyMarkup: null,
                                cancellationToken: cancellationToken);
                            
                            await botClient.SendMessage(
                                callbackQuery.Message.Chat.Id,
                                $"✅ #{invoiceId} raqamli invoys tasdiqlandi.",
                                replyParameters: callbackQuery.Message.MessageId,
                                cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        await botClient.AnswerCallbackQuery(callbackQuery.Id, "Xatolik yuz berdi yoki invoys allaqachon to'langan.", showAlert: true, cancellationToken: cancellationToken);
                    }
                }
                else if (action == "reject")
                {
                    // For now, just answer the query. You could add RejectInvoiceAsync to IBillingService.
                    await botClient.AnswerCallbackQuery(callbackQuery.Id, "Invoys rad etildi.", cancellationToken: cancellationToken);
                    
                    if (callbackQuery.Message != null)
                    {
                        await botClient.EditMessageReplyMarkup(
                            callbackQuery.Message.Chat.Id,
                            callbackQuery.Message.MessageId,
                            replyMarkup: null,
                            cancellationToken: cancellationToken);
                        
                        await botClient.SendMessage(
                            callbackQuery.Message.Chat.Id,
                            $"❌ #{invoiceId} raqamli invoys rad etildi.",
                            replyParameters: callbackQuery.Message.MessageId,
                            cancellationToken: cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling callback query");
                await botClient.AnswerCallbackQuery(callbackQuery.Id, "Tizim xatoligi yuz berdi.", showAlert: true, cancellationToken: cancellationToken);
            }
        }

        

        public async Task SendInvoiceForApprovalAsync(int invoiceId, string cafeName, decimal amount, string receiptUrl, string invoiceNumber)
        {
            if (_botClient == null || string.IsNullOrEmpty(_adminChatId)) return;

            var message = $"🧾 <b>Yangi Invoys to'lovi kiritildi!</b>\n\n" +
                          $"🏢 Kafe: <b>{cafeName}</b>\n" +
                          $"📄 Invoys raqami: #{invoiceNumber}\n" +
                          $"💰 Summa: {amount:N0} UZS\n\n" +
                          $"Tasdiqlaysizmi?";

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData("✅ Tasdiqlash", $"invoice_approve_{invoiceId}"),
                    InlineKeyboardButton.WithCallbackData("❌ Bekor qilish", $"invoice_reject_{invoiceId}")
                }
            });

            try
            {
                // Note: The receiptUrl in a real app would be a full URL accessible from internet. 
                // Alternatively, if it's a local file, we would upload it. 
                // Assuming it's an absolute path to the physical file on the server.
                var physicalPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", receiptUrl.TrimStart('/'));
                
                if (System.IO.File.Exists(physicalPath))
                {
                    using var stream = System.IO.File.OpenRead(physicalPath);
                    var inputFile = InputFile.FromStream(stream, System.IO.Path.GetFileName(physicalPath));
                    
                    await _botClient.SendPhoto(
                        chatId: _adminChatId,
                        photo: inputFile,
                        caption: message,
                        parseMode: ParseMode.Html,
                        replyMarkup: inlineKeyboard);
                }
                else
                {
                    // Just text if file not found
                    await _botClient.SendMessage(
                        chatId: _adminChatId,
                        text: message,
                        parseMode: ParseMode.Html,
                        replyMarkup: inlineKeyboard);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invoice approval to Telegram.");
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (_botClient == null || string.IsNullOrEmpty(_adminChatId)) return;
            
            try
            {
                await _botClient.SendMessage(
                    chatId: _adminChatId,
                    text: message,
                    parseMode: ParseMode.Html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to Telegram.");
            }
        }
    }
}
