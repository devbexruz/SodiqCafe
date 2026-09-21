using System;

namespace SodiqCafeMVC.Domain.Entities
{
    public class DailySnapshot
    {
        public int Id { get; set; }
        
        public int CafeId { get; set; }
        public Cafe Cafe { get; set; } = null!;

        public DateTime Date { get; set; }
        
        public int TotalChecks { get; set; }
        public int TotalBonusesGiven { get; set; }
        
        public string? BonusesJson { get; set; } // Qaysi bonusdan nechta berilgani haqida JSON

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
