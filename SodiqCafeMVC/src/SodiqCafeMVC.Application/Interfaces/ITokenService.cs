using System;
using System.Security.Claims;
using SodiqCafeMVC.Domain.Entities;

namespace SodiqCafeMVC.Application.Interfaces
{
    public interface ITokenService
    {
        // Tasodifiy sessiya tokeni yaratish (Random cryptographic token)
        string GenerateRandomSessionToken();

        // Refresh token yaratish (amal qilish muddati 1 oy)
        string GenerateRefreshToken();

        // Access token yaratish (amal qilish muddati 1 soat)
        string GenerateAccessToken(User user, string sessionToken);

        // Muddati o'tgan yoki faol tokendan Claimslarni olish
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

        TimeSpan AccessTokenLifetime { get; } // 1 soat (TimeSpan.FromHours(1))
        TimeSpan RefreshTokenLifetime { get; } // 1 oy (TimeSpan.FromDays(30))
    }
}
