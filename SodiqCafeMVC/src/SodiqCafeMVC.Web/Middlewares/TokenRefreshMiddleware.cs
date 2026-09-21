using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SodiqCafeMVC.Application.Interfaces;
using SodiqCafeMVC.Web.Services;

namespace SodiqCafeMVC.Web.Middlewares
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenRefreshMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService, ICookieService cookieService)
        {
            var accessToken = cookieService.GetAccessToken(context.Request);
            var refreshToken = cookieService.GetRefreshToken(context.Request);
            var sessionToken = cookieService.GetSessionToken(context.Request);

            // Agar Access Token yo'q bo'lsa yoki uning muddati tugagan bo'lsa, lekin Refresh & Session tokenlar bo'lsa
            bool shouldRefresh = false;

            if (!string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(sessionToken))
            {
                if (string.IsNullOrEmpty(accessToken))
                {
                    shouldRefresh = true;
                }
                else
                {
                    try
                    {
                        var handler = new JwtSecurityTokenHandler();
                        if (handler.CanReadToken(accessToken))
                        {
                            var jwtToken = handler.ReadJwtToken(accessToken);
                            // Agar tokenga 2 daqiqadan kam vaqt qolgan bo'lsa yoki o'tgan bo'lsa
                            if (jwtToken.ValidTo <= DateTime.UtcNow.AddMinutes(2))
                            {
                                shouldRefresh = true;
                            }
                        }
                        else
                        {
                            shouldRefresh = true;
                        }
                    }
                    catch
                    {
                        shouldRefresh = true;
                    }
                }
            }

            if (shouldRefresh && !string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(sessionToken))
            {
                var ip = context.Connection.RemoteIpAddress?.ToString();
                var userAgent = context.Request.Headers["User-Agent"].ToString();

                var refreshResult = await authService.RefreshTokenAsync(sessionToken, refreshToken, ip, userAgent);
                if (refreshResult.Success && !string.IsNullOrEmpty(refreshResult.AccessToken) && !string.IsNullOrEmpty(refreshResult.RefreshToken))
                {
                    // Yangi HttpOnly cookielarni joylash
                    cookieService.SetAuthCookies(
                        context.Response,
                        context.Request,
                        refreshResult.SessionToken ?? sessionToken,
                        refreshResult.AccessToken,
                        refreshResult.RefreshToken
                    );

                    // Joriy kontekstda ham yangi tokenni ishlatish uchun cookie qiymatini yangilaymiz
                    context.Request.Headers.Append("Authorization", $"Bearer {refreshResult.AccessToken}");
                }
            }

            await _next(context);
        }
    }
}
