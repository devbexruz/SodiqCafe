using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SodiqCafeMVC.Application.Interfaces
{
    public interface ICafeTelegramBotClientManager
    {
        ITelegramBotClient GetClient(long cafeId, string botToken);
        void RemoveClient(long cafeId);

    }

}