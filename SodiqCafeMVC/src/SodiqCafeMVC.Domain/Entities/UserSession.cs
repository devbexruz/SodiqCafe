using System;

namespace SodiqCafeMVC.Domain.Entities
{
    public class UserSession
    {
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Tasodifiy sessiya tokeni (Random Session Token / Identifier)
        public string SessionToken { get; set; } = string.Empty;

        // Refresh token (amal qilish muddati 1 oy)
        public string RefreshToken { get; set; } = string.Empty;

        // Access token (amal qilish muddati 1 soat)
        public string? AccessToken { get; set; }

        public DateTime AccessTokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUsedAt { get; set; }

        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }

        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public bool IsActive => !IsRevoked && DateTime.UtcNow <= RefreshTokenExpiresAt;
    }
}
