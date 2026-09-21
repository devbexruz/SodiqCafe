using System;
using System.ComponentModel.DataAnnotations;

namespace SodiqCafeMVC.Domain.Entities
{
    public class SystemSettings
    {
        [Key]
        public int Id { get; set; }

        // Pricing Configuration
        public decimal BasePrice { get; set; } = 120000m;
        public int BaseUsersLimit { get; set; } = 500;
        public decimal PricePerExtraUsers { get; set; } = 12000m;
        public int ExtraUsersStep { get; set; } = 100;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
