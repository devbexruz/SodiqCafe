using System;
using SodiqCafeMVC.Domain.Enums;

namespace SodiqCafeMVC.Domain.Entities
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        public int CafeId { get; set; }
        public Cafe Cafe { get; set; } = null!;

        public int? UserId { get; set; }
        public User? User { get; set; }

        public int DailyOrderNumber { get; set; }
        
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Waiting;
        public string? Token { get; set; }
        
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
