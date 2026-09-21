using System;
using SodiqCafeMVC.Domain.Enums;

namespace SodiqCafeMVC.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Role Role { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public decimal Balance { get; set; } // Yangi qo'shildi
        public List<CafeDto> Cafes { get; set; } = new List<CafeDto>();
    }

    public class CafeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LogoUrl { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Yangi billing va faollik xususiyatlari
        public DateTime ActiveUntil { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsSuspended { get; set; }
        public bool IsActive => !IsSuspended && DateTime.UtcNow <= ActiveUntil;
        public int DaysRemaining => Math.Max(0, (int)Math.Ceiling((ActiveUntil - DateTime.UtcNow).TotalDays));
        public int PendingInvoicesCount { get; set; }
    }

    public class UserSessionDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string SessionToken { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime AccessTokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsRevoked { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class InvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int CafeId { get; set; }
        public string CafeName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? Notes { get; set; }
        public bool CanBePaid { get; set; }
    }

    public class TopUpBalanceDto
    {
        public int CafeId { get; set; }
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }

    public class BillingSummaryDto
    {
        public int TotalCafes { get; set; }
        public int ActiveCafes { get; set; }
        public int SuspendedCafes { get; set; }
        public int PendingInvoicesCount { get; set; }
        public decimal TotalPendingInvoicesAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalCafesBalance { get; set; }
    }
}
