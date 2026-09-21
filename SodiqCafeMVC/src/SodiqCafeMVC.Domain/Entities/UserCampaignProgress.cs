using System;

namespace SodiqCafeMVC.Domain.Entities
{
    public class UserCampaignProgress
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int BonusCampaignId { get; set; }
        public BonusCampaign BonusCampaign { get; set; } = null!;

        public int CurrentValue { get; set; } = 0; // Hozirda nechta katakcha to'lgan
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
