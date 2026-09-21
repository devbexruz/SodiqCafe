using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SodiqCafeMVC.Application.Interfaces
{
    public interface ISetTelegramBotWebhook
    {
        Task SetWebhookAsync(string webhookUrl, string botToken, string? secretToken = null); 
    }
}