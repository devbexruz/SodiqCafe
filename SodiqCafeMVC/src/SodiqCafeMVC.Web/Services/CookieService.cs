using System;
using Microsoft.AspNetCore.Http;

namespace SodiqCafeMVC.Web.Services
{
    public interface ICookieService
    {
        void SetAuthCookies(HttpResponse response, HttpRequest request, string sessionToken, string accessToken, string refreshToken);
        void ClearAuthCookies(HttpResponse response, HttpRequest request);
        string? GetAccessToken(HttpRequest request);
        string? GetRefreshToken(HttpRequest request);
        string? GetSessionToken(HttpRequest request);
    }

    public class CookieService : ICookieService
    {
        public const string AccessTokenCookieName = "X-Access-Token";
        public const string RefreshTokenCookieName = "X-Refresh-Token";
        public const string SessionTokenCookieName = "X-Session-Token";

        // Access token: 1 soat
        private static readonly TimeSpan AccessTokenDuration = TimeSpan.FromHours(1);
        // Refresh token: 1 oy (30 kun)
        private static readonly TimeSpan RefreshTokenDuration = TimeSpan.FromDays(30);

        public void SetAuthCookies(HttpResponse response, HttpRequest request, string sessionToken, string accessToken, string refreshToken)
        {
            var isHttps = request.IsHttps;

            // 1. Access Token Cookie - 1 soat, HttpOnly
            var accessCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.Add(AccessTokenDuration),
                IsEssential = true
            };
            response.Cookies.Append(AccessTokenCookieName, accessToken, accessCookieOptions);

            // 2. Refresh Token Cookie - 1 oy, HttpOnly
            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.Add(RefreshTokenDuration),
                IsEssential = true
            };
            response.Cookies.Append(RefreshTokenCookieName, refreshToken, refreshCookieOptions);

            // 3. Session Token Cookie (Tasodifiy token) - 1 oy, HttpOnly
            var sessionCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.Add(RefreshTokenDuration),
                IsEssential = true
            };
            response.Cookies.Append(SessionTokenCookieName, sessionToken, sessionCookieOptions);
        }

        public void ClearAuthCookies(HttpResponse response, HttpRequest request)
        {
            var isHttps = request.IsHttps;
            var expiredCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(-1),
                IsEssential = true
            };

            response.Cookies.Delete(AccessTokenCookieName, expiredCookieOptions);
            response.Cookies.Delete(RefreshTokenCookieName, expiredCookieOptions);
            response.Cookies.Delete(SessionTokenCookieName, expiredCookieOptions);
        }

        public string? GetAccessToken(HttpRequest request) =>
            request.Cookies[AccessTokenCookieName];

        public string? GetRefreshToken(HttpRequest request) =>
            request.Cookies[RefreshTokenCookieName];

        public string? GetSessionToken(HttpRequest request) =>
            request.Cookies[SessionTokenCookieName];
    }
}
