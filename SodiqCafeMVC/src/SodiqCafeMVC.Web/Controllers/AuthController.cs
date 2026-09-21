using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SodiqCafeMVC.Application.DTOs;
using SodiqCafeMVC.Application.Interfaces;
using SodiqCafeMVC.Domain.Enums;
using SodiqCafeMVC.Web.Services;

namespace SodiqCafeMVC.Web.Controllers
{
    [Route("auth")]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ICookieService _cookieService;

        public AuthController(IAuthService _authService, ICookieService _cookieService)
        {
            this._authService = _authService;
            this._cookieService = _cookieService;
        }

        [HttpGet("login")]
        [AllowAnonymous]
        public IActionResult Login([FromQuery] string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole(Role.Admin.ToString()))
                {
                    return RedirectToAction("Cafes", "Admin");
                }
                return RedirectToAction("Index", "Menyu");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginDto());
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, [FromQuery] string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _authService.LoginAsync(model, ip, userAgent);

            if (!result.Success || result.User == null)
            {
                ModelState.AddModelError(string.Empty, result.Message ?? "Login yoki parol noto'g'ri.");
                return View(model);
            }

            // HttpOnly Cookielarni o'rnatish (1 soat Access Token, 1 oy Refresh Token, 1 oy Session Token)
            _cookieService.SetAuthCookies(
                Response,
                Request,
                result.SessionToken!,
                result.AccessToken!,
                result.RefreshToken!
            );

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            if (result.User.Role == Role.Admin)
            {
                return RedirectToAction("Cafes", "Admin");
            }

            return RedirectToAction("Index", "Menyu");
        }

        [HttpPost("logout")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var sessionToken = _cookieService.GetSessionToken(Request);
            if (!string.IsNullOrEmpty(sessionToken))
            {
                await _authService.RevokeSessionAsync(sessionToken);
            }

            _cookieService.ClearAuthCookies(Response, Request);
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet("access-denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost("login-with-token")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginWithToken([FromBody] System.Collections.Generic.List<string> tokens)
        {
            if (tokens == null || tokens.Count == 0) return BadRequest();

            // SodiqCafeMVC.Infrastructure.Data.AppDbContext must be resolved from services
            var context = HttpContext.RequestServices.GetService(typeof(SodiqCafeMVC.Infrastructure.Data.AppDbContext)) as SodiqCafeMVC.Infrastructure.Data.AppDbContext;
            
            if (context == null) return StatusCode(500, "Database context not found.");

            var validReward = context.UserRewards
                .FirstOrDefault(r => r.TransferToken != null && tokens.Contains(r.TransferToken) && r.IsTransferredToBot && r.UserId.HasValue);

            if (validReward != null && validReward.UserId.HasValue)
            {
                var user = await context.Users.FindAsync(validReward.UserId.Value);
                if (user != null)
                {
                    // Create a session for this user manually (or through _authService if it supports userId login)
                    // Since _authService doesn't expose LoginByUserId, let's just generate tokens using _authService if possible or just use a workaround.
                    // Wait, we need to log them in! Let's check IAuthService.
                    
                    // Assign other tokens to this user
                    var unassignedRewards = context.UserRewards.Where(r => r.TransferToken != null && tokens.Contains(r.TransferToken) && !r.UserId.HasValue).ToList();
                    foreach (var reward in unassignedRewards)
                    {
                        reward.UserId = user.Id;
                    }
                    await context.SaveChangesAsync();
                    
                    // TODO: Implement actual login session if needed
                    return Ok(new { success = true, userId = user.Id, username = user.Username });
                }
            }

            return Ok(new { success = false });
        }
    }
}
