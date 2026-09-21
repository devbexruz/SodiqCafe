using System;
using System.Collections.Generic;
using SodiqCafeMVC.Domain.Enums;

namespace SodiqCafeMVC.Domain.Entities
{
    public class Cafe
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LogoUrl { get; set; }
        public Role Role { get; set; } = Role.CafeOwner;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Kafe egasi (User)
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        // Faollik muddati (Active Date / Expiration Date)
        public DateTime ActiveUntil { get; set; } = DateTime.UtcNow.AddMonths(1);

        // Kafe balansi olib tashlandi, endi User modelida bo'ladi

        // B2B Billing (Oylik abonent to'lovi)
        public decimal BasePrice { get; set; } = 120000m;
        public int BaseUsersLimit { get; set; } = 500;
        public decimal PricePerExtraUsers { get; set; } = 12000m;
        public int ExtraUsersStep { get; set; } = 100;
        public bool HasCustomPrice { get; set; } = false;

        // Vaqtincha to'xtatilganlik holati (to'lov qilinmasa yoki admin to'xtatsa)
        public bool IsSuspended { get; set; } = false;

        // Kafe faol ishlayotganini tekshirish
        public bool IsActive => !IsSuspended && DateTime.UtcNow <= ActiveUntil;

        // Invoyslar tarixi
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

        // To'lov operatsiyalari tarixi
        public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();

        // Mahsulotlar (Menu)
        public ICollection<Product> Products { get; set; } = new List<Product>();

        // Kafening mijozlari
        public ICollection<CafeUser> CafeUsers { get; set; } = new List<CafeUser>();

        // Narx o'zgarishlari tarixi
        public ICollection<CafePriceChangeHistory> PriceChangeHistories { get; set; } = new List<CafePriceChangeHistory>();

        // Aksiya va bonuslar
        public ICollection<BonusCampaign> BonusCampaigns { get; set; } = new List<BonusCampaign>();
        public ICollection<UserReward> UserRewards { get; set; } = new List<UserReward>();

        // Navbatdagi so'rovlar
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        // Telegram Bot
        public string? TelegramBotToken { get; set; }
        public string? TelegramBotUsername { get; set; } 
        public string? TelegramWebhookSecretToken { get; set; }
    }
}