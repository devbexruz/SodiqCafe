using System;
using System.Collections.Generic;
using SodiqCafeMVC.Domain.Enums;

namespace SodiqCafeMVC.Domain.Entities
{
    public class BonusCampaign
    {
        public int Id { get; set; }
        
        public int CafeId { get; set; }
        public Cafe Cafe { get; set; } = null!;

        public string Name { get; set; } = string.Empty; // Masalan: "10-kofe bepul"
        public string? Description { get; set; }
        
        public BonusType Type { get; set; }
        
        // Shart qiymati (Katakchalar soni, kunlar soni, k-chi mijoz, min summa)
        public decimal ConditionValue { get; set; }
        
        // Nima berilishi
        public string RewardDescription { get; set; } = string.Empty;

        public bool IsVisible { get; set; } = true;
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserCampaignProgress> Progresses { get; set; } = new List<UserCampaignProgress>();
        public ICollection<UserReward> Rewards { get; set; } = new List<UserReward>();
    }
}
