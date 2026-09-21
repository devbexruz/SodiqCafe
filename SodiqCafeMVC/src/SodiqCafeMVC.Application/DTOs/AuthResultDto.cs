using System;

namespace SodiqCafeMVC.Application.DTOs
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        // Tasodifiy sessiya tokeni
        public string? SessionToken { get; set; }

        // JWT Access token (1 soat)
        public string? AccessToken { get; set; }

        // Refresh token (1 oy)
        public string? RefreshToken { get; set; }

        public DateTime? AccessTokenExpiresAt { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }

        public UserDto? User { get; set; }

        public static AuthResultDto Failed(string message) =>
            new() { Success = false, Message = message };

        public static AuthResultDto Succeeded(
            string sessionToken,
            string accessToken,
            string refreshToken,
            DateTime accessTokenExpiresAt,
            DateTime refreshTokenExpiresAt,
            UserDto user,
            string? message = null) =>
            new()
            {
                Success = true,
                SessionToken = sessionToken,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt,
                User = user,
                Message = message
            };
    }
}
