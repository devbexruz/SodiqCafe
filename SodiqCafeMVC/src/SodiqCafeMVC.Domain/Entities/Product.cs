using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SodiqCafeMVC.Domain.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public SodiqCafeMVC.Domain.Enums.ProductCategory Category { get; set; } = SodiqCafeMVC.Domain.Enums.ProductCategory.Drinks;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsDefaultTemplate { get; set; } = false;

        public bool IsRecommended { get; set; } = false;

        public int? CafeId { get; set; }
        
        [ForeignKey(nameof(CafeId))]
        public Cafe? Cafe { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
