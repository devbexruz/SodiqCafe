using System;
using SodiqCafeMVC.Domain.Enums;

namespace SodiqCafeMVC.Domain.Entities
{
    public class Invoice
    {
        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public int CafeId { get; set; }
        public Cafe Cafe { get; set; } = null!;

        // Invoys summasi
        public decimal Amount { get; set; }

        // O'tgan oydagi aktiv mijozlar soni
        public int TotalActiveUsers { get; set; }

        // To'lov cheki skrinshoti
        public string? ReceiptImageUrl { get; set; }

        // Hisob-kitob davri: oyning 1-sanasidan oxirgi sanasigacha
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }

        // To'lovning so'nggi muddati
        public DateTime DueDate { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        public string? Notes { get; set; }
    }
}
