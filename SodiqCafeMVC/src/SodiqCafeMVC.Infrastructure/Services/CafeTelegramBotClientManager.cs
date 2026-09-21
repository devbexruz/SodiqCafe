using System.Collections.Concurrent;
using System.Net.Http;
using SodiqCafeMVC.Application.Interfaces;
using Telegram.Bot;

namespace SodiqCafeMVC.Infrastructure.Services
{
    public class CafeTelegramBotClientManager : ICafeTelegramBotClientManager
    {
        // 1. Thread-safe (Xavfsiz) lug'at ishlatamiz va static'ni olib tashlaymiz
        private readonly ConcurrentDictionary<long, ITelegramBotClient> _botClients = new();
        
        private readonly IHttpClientFactory _httpClientFactory;

        public CafeTelegramBotClientManager(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Metod nomini ICafeTelegramBotClient interfeysida ham xuddi shunday to'g'rilab qo'yasiz
        public ITelegramBotClient GetClient(long cafeId, string botToken)
        {
            // 2. GetOrAdd: Agar bor bo'lsa darhol qaytaradi, yo'q bo'lsa xavfsiz yaratib qo'shadi
            return _botClients.GetOrAdd(cafeId, id =>
            {
                // 3. Umumiy trubadan foydalanamiz
                var httpClient = _httpClientFactory.CreateClient("TelegramCafeClient");
                return new TelegramBotClient(botToken, httpClient);
            });
        }

        // 4. QO'SHIMCHA YENGILLIK: Kafe egasi tokenini yangilasa, eski botni xotiradan tozalash uchun
        public void RemoveClient(long cafeId)
        {
            _botClients.TryRemove(cafeId, out _);
        }
    }
}