using System;
using SodiqCafeMVC.Domain.Enums;

namespace SodiqCafeMVC.Domain.Entities
{
    public class PaymentTransaction
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public int? CafeId { get; set; }
        public Cafe? Cafe { get; set; }

        public int? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        public decimal Amount { get; set; }
        public PaymentType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
    }
}
