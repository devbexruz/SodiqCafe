using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SodiqCafeMVC.Application.DTOs;
using SodiqCafeMVC.Application.Interfaces;

namespace SodiqCafeMVC.Web.ApiControllers;

[Route("api/v1/auth")]
public class AuthV1ApiController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthV1ApiController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        if (!ModelState.IsValid)
            return ApiFail("Noto'g'ri so'rov yuborildi");

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _authService.LoginAsync(model, ip, userAgent);

        if (!result.Success)
            return ApiFail(result.Message ?? "Login xato");

        return ApiOk(result, "Tizimga muvaffaqiyatli kirdingiz");
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.SessionToken) || string.IsNullOrEmpty(request.RefreshToken))
            return ApiFail("Tokenlar yetishmayapti");

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _authService.RefreshTokenAsync(request.SessionToken, request.RefreshToken, ip, userAgent);

        if (!result.Success)
            return ApiFail(result.Message ?? "Token yangilash xato");

        return ApiOk(result, "Token muvaffaqiyatli yangilandi");
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        if (string.IsNullOrEmpty(request.SessionToken))
            return ApiFail("Sessiya tokeni yo'q");

        await _authService.RevokeSessionAsync(request.SessionToken);
        return ApiOk<object>(null, "Tizimdan chiqish muvaffaqiyatli");
    }

    public class RefreshTokenRequest
    {
        public string SessionToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class LogoutRequest
    {
        public string SessionToken { get; set; } = null!;
    }
}
