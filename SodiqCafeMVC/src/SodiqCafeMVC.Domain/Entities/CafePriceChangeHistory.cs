using System;

namespace SodiqCafeMVC.Domain.Entities
{
    public class CafePriceChangeHistory
    {
        public int Id { get; set; }

        public int CafeId { get; set; }
        public Cafe Cafe { get; set; } = null!;

        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string Reason { get; set; } = string.Empty;

        public int AdminId { get; set; }
        public User Admin { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
