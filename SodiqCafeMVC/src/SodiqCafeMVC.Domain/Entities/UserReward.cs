using System;

namespace SodiqCafeMVC.Domain.Entities
{
    public class UserReward
    {
        public int Id { get; set; }
        
        public int? UserId { get; set; }
        public User? User { get; set; }

        public int CafeId { get; set; }
        public Cafe Cafe { get; set; } = null!;

        public int BonusCampaignId { get; set; }
        public BonusCampaign BonusCampaign { get; set; } = null!;

        public bool IsUsed { get; set; } = false; // Ishlatildimi
        
        public string? TransferToken { get; set; }
        public bool IsTransferredToBot { get; set; } = false;

        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UsedAt { get; set; }
    }
}
