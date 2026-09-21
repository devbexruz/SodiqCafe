using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SodiqCafeMVC.Application.DTOs;
using SodiqCafeMVC.Application.Interfaces;
using SodiqCafeMVC.Web.Services;

namespace SodiqCafeMVC.Web.ApiControllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ICookieService _cookieService;

        public AuthApiController(IAuthService authService, ICookieService cookieService)
        {
            _authService = authService;
            _cookieService = cookieService;
        }

        // Login endpoint
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _authService.LoginAsync(dto, ip, userAgent);
            if (!result.Success)
            {
                return Unauthorized(new { message = result.Message });
            }

            // HttpOnly Cookie larni o'rnatish
            _cookieService.SetAuthCookies(
                Response,
                Request,
                result.SessionToken!,
                result.AccessToken!,
                result.RefreshToken!
            );

            return Ok(result);
        }

        // Faqat Admin kafe egasini ro'yxatdan o'tkazishi mumkin
        [HttpPost("register-cafe-owner")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RegisterCafeOwner([FromBody] RegisterOwnerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var adminIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(adminIdClaim, out var adminId))
            {
                return Unauthorized(new { message = "Admin identifikatori topilmadi" });
            }

            var result = await _authService.RegisterCafeOwnerAsync(dto, adminId);
            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(result);
        }

        // Refresh token endpoint
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto? dto)
        {
            var sessionToken = dto?.SessionToken ?? _cookieService.GetSessionToken(Request);
            var refreshToken = dto?.RefreshToken ?? _cookieService.GetRefreshToken(Request);

            if (string.IsNullOrEmpty(sessionToken) || string.IsNullOrEmpty(refreshToken))
            {
                return BadRequest(new { message = "Sessiya tokeni yoki Refresh token topilmadi." });
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _authService.RefreshTokenAsync(sessionToken, refreshToken, ip, userAgent);
            if (!result.Success)
            {
                return Unauthorized(new { message = result.Message });
            }

            // HttpOnly cookie larni yangilash
            _cookieService.SetAuthCookies(
                Response,
                Request,
                result.SessionToken!,
                result.AccessToken!,
                result.RefreshToken!
            );

            return Ok(result);
        }

        // Logout endpoint
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var sessionToken = _cookieService.GetSessionToken(Request);
            if (!string.IsNullOrEmpty(sessionToken))
            {
                await _authService.RevokeSessionAsync(sessionToken);
            }

            _cookieService.ClearAuthCookies(Response, Request);
            return Ok(new { message = "Muvaffaqiyatli tizimdan chiqildi." });
        }

        // Joriy foydalanuvchi ma'lumotlari
        [HttpGet("me")]
        [Authorize]

        public async Task<IActionResult> Me()
        {

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var username = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);

            var fullName = User.FindFirstValue("FullName");

            // User's cafes from DB
            var dbContext = HttpContext.RequestServices.GetService(typeof(SodiqCafeMVC.Application.Interfaces.IAppDbContext)) as SodiqCafeMVC.Application.Interfaces.IAppDbContext;
            
            var userCafes = new List<object>();
            int? defaultCafeId = null;

            if (dbContext != null)
            {
                var cafes = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(
                    System.Linq.Queryable.OrderByDescending(
                        System.Linq.Queryable.Where(dbContext.Cafes, c => c.OwnerId == userId),
                        c => c.CreatedAt
                    )
                );

                foreach (var c in cafes)
                {
                    userCafes.Add(new { Id = c.Id, Name = c.Name });
                }

                if (cafes.Count > 0)
                {
                    defaultCafeId = cafes[0].Id;
                }
            }

            return Ok(new
            {
                userId,
                username,
                email,
                role,


                fullName,
                defaultCafeId,
                cafes = userCafes
            });
        }
    }

    public class RefreshRequestDto
    {
        public string? SessionToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
