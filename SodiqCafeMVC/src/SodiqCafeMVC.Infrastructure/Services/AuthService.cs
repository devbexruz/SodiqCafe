using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SodiqCafeMVC.Application.DTOs;
using SodiqCafeMVC.Application.Interfaces;
using SodiqCafeMVC.Domain.Entities;
using SodiqCafeMVC.Domain.Enums;
using SodiqCafeMVC.Infrastructure.Data;

namespace SodiqCafeMVC.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // Kafe egasini ro'yxatdan o'tkazish (Faqat Admin uchun)
        public async Task<AuthResultDto> RegisterCafeOwnerAsync(RegisterOwnerDto dto, int adminUserId)
        {
            // 1. Adminni tekshirish
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminUserId);
            if (adminUser == null || adminUser.Role != Role.Admin)
            {
                return AuthResultDto.Failed("Ruxsat berilmadi! Faqat tizim administratori kafe egasini ro'yxatdan o'tkaza oladi.");
            }

            // 2. Username va Email band emasligini tekshirish
            var normalizedUsername = dto.Username.Trim().ToLowerInvariant();
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername))
            {
                return AuthResultDto.Failed($"'{dto.Username}' nomli foydalanuvchi allaqachon mavjud.");
            }

            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
            {
                return AuthResultDto.Failed($"'{dto.Email}' elektron pochta manzili allaqachon ro'yxatdan o'tgan.");
            }

            // 3. Yangi Kafe egasi (User) yaratish
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var cafeOwner = new User
            {
                Username = dto.Username.Trim(),
                Email = dto.Email.Trim(),
                PasswordHash = passwordHash,
                Role = Role.CafeOwner,
                FullName = dto.FullName.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(cafeOwner);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = cafeOwner.Id,
                Username = cafeOwner.Username,
                Email = cafeOwner.Email,
                Role = cafeOwner.Role,
                FullName = cafeOwner.FullName,
                PhoneNumber = cafeOwner.PhoneNumber,
                Balance = cafeOwner.Balance,
                Cafes = new List<CafeDto>()
            };

            return new AuthResultDto
            {
                Success = true,
                User = userDto,
                Message = $"Kafe egasi '{cafeOwner.Username}' muvaffaqiyatli ro'yxatdan o'tkazildi."
            };
        }

        public async Task<AuthResultDto> AddCafeToOwnerAsync(int ownerId, RegisterCafeDto dto, int adminUserId)
        {
            var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminUserId);
            if (adminUser == null || adminUser.Role != Role.Admin)
            {
                return AuthResultDto.Failed("Ruxsat berilmadi! Faqat tizim administratori kafe qo'sha oladi.");
            }

            var cafeOwner = await _context.Users.Include(u => u.Cafes).FirstOrDefaultAsync(u => u.Id == ownerId);
            if (cafeOwner == null)
            {
                return AuthResultDto.Failed("Foydalanuvchi topilmadi.");
            }

            var cafe = new Cafe
            {
                Name = dto.CafeName.Trim(),
                Address = dto.CafeAddress.Trim(),
                PhoneNumber = dto.CafePhone?.Trim() ?? cafeOwner.PhoneNumber?.Trim(),
                Role = Role.CafeOwner,
                OwnerId = cafeOwner.Id,
                BasePrice = dto.BasePrice > 0 ? dto.BasePrice : 300000m,
                ActiveUntil = DateTime.UtcNow.AddMonths(1),
                IsSuspended = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Cafes.AddAsync(cafe);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = cafeOwner.Id,
                Username = cafeOwner.Username,
                Email = cafeOwner.Email,
                Role = cafeOwner.Role,
                FullName = cafeOwner.FullName,
                PhoneNumber = cafeOwner.PhoneNumber,
                Balance = cafeOwner.Balance,
                Cafes = cafeOwner.Cafes.Select(c => new CafeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    LogoUrl = c.LogoUrl,
                    OwnerId = c.OwnerId,
                    OwnerName = c.Owner.FullName ?? c.Owner.Username,
                    OwnerEmail = c.Owner.Email,
                    CreatedAt = c.CreatedAt,
                    ActiveUntil = c.ActiveUntil,
                    BasePrice = c.BasePrice,
                    IsSuspended = c.IsSuspended,
                    PendingInvoicesCount = c.Invoices.Count(i => i.Status == InvoiceStatus.Pending)
                }).ToList()
            };

            return new AuthResultDto
            {
                Success = true,
                User = userDto,
                Message = $"Yangi '{cafe.Name}' kafesi muvaffaqiyatli qo'shildi."
            };
        }

        // Tizimga kirish (Login) -> Sessiya, Tasodifiy token, Access token (1 soat), Refresh token (1 oy)
        public async Task<AuthResultDto> LoginAsync(LoginDto dto, string? ipAddress, string? userAgent)
        {
            var search = dto.UsernameOrEmail.Trim().ToLowerInvariant();
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == search || u.Email.ToLower() == search);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return AuthResultDto.Failed("Foydalanuvchi nomi yoki parol noto'g'ri!");
            }

            // 1. Tasodifiy sessiya tokeni yaratish
            var sessionToken = _tokenService.GenerateRandomSessionToken();

            // 2. Refresh token yaratish (1 oy)
            var refreshToken = _tokenService.GenerateRefreshToken();

            // 3. Access token yaratish (1 soat)
            var accessToken = _tokenService.GenerateAccessToken(user, sessionToken);

            var now = DateTime.UtcNow;
            var accessExpiresAt = now.Add(_tokenService.AccessTokenLifetime);
            var refreshExpiresAt = now.Add(_tokenService.RefreshTokenLifetime);

            // 4. Sessiyani DB ga saqlash
            var session = new UserSession
            {
                UserId = user.Id,
                SessionToken = sessionToken,
                RefreshToken = refreshToken,
                AccessToken = accessToken,
                AccessTokenExpiresAt = accessExpiresAt,
                RefreshTokenExpiresAt = refreshExpiresAt,
                CreatedAt = now,
                LastUsedAt = now,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsRevoked = false
            };

            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Balance = user.Balance,
                Cafes = user.Cafes.Select(c => new CafeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    LogoUrl = c.LogoUrl,
                    OwnerId = c.OwnerId,
                    OwnerName = c.Owner.FullName ?? c.Owner.Username,
                    OwnerEmail = c.Owner.Email,
                    CreatedAt = c.CreatedAt,
                    ActiveUntil = c.ActiveUntil,
                    BasePrice = c.BasePrice,
                    IsSuspended = c.IsSuspended,
                    PendingInvoicesCount = c.Invoices.Count(i => i.Status == InvoiceStatus.Pending)
                }).ToList()
            };

            return AuthResultDto.Succeeded(
                sessionToken: sessionToken,
                accessToken: accessToken,
                refreshToken: refreshToken,
                accessTokenExpiresAt: accessExpiresAt,
                refreshTokenExpiresAt: refreshExpiresAt,
                user: userDto,
                message: "Muvaffaqiyatli tizimga kirdingiz."
            );
        }

        // Sessiyani yangilash (Refresh)
        public async Task<AuthResultDto> RefreshTokenAsync(string sessionToken, string refreshToken, string? ipAddress, string? userAgent)
        {
            if (string.IsNullOrWhiteSpace(sessionToken) || string.IsNullOrWhiteSpace(refreshToken))
            {
                return AuthResultDto.Failed("Sessiya ma'lumotlari to'liq emas.");
            }

            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken && s.RefreshToken == refreshToken);

            if (session == null || session.IsRevoked)
            {
                return AuthResultDto.Failed("Sessiya bekor qilingan yoki topilmadi.");
            }

            if (DateTime.UtcNow > session.RefreshTokenExpiresAt)
            {
                session.IsRevoked = true;
                session.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return AuthResultDto.Failed("Refresh token muddati tugagan (1 oy o'tdi). Qaytadan kiring.");
            }

            // Yangi tokenlar yaratish (Token rotation)
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var newAccessToken = _tokenService.GenerateAccessToken(session.User, session.SessionToken);

            var now = DateTime.UtcNow;
            var accessExpiresAt = now.Add(_tokenService.AccessTokenLifetime);
            var refreshExpiresAt = now.Add(_tokenService.RefreshTokenLifetime);

            // Sessiyani yangilash
            session.RefreshToken = newRefreshToken;
            session.RefreshTokenExpiresAt = refreshExpiresAt;
            session.AccessToken = newAccessToken;
            session.AccessTokenExpiresAt = accessExpiresAt;
            session.LastUsedAt = now;
            session.IpAddress = ipAddress ?? session.IpAddress;
            session.UserAgent = userAgent ?? session.UserAgent;

            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = session.User.Id,
                Username = session.User.Username,
                Email = session.User.Email,
                Role = session.User.Role,
                FullName = session.User.FullName,
                PhoneNumber = session.User.PhoneNumber,
                Balance = session.User.Balance,
                Cafes = session.User.Cafes.Select(c => new CafeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    LogoUrl = c.LogoUrl,
                    OwnerId = c.OwnerId,
                    OwnerName = c.Owner.FullName ?? c.Owner.Username,
                    OwnerEmail = c.Owner.Email,
                    CreatedAt = c.CreatedAt,
                    ActiveUntil = c.ActiveUntil,
                    BasePrice = c.BasePrice,
                    IsSuspended = c.IsSuspended,
                    PendingInvoicesCount = c.Invoices.Count(i => i.Status == InvoiceStatus.Pending)
                }).ToList()
            };

            return AuthResultDto.Succeeded(
                sessionToken: session.SessionToken,
                accessToken: newAccessToken,
                refreshToken: newRefreshToken,
                accessTokenExpiresAt: accessExpiresAt,
                refreshTokenExpiresAt: refreshExpiresAt,
                user: userDto,
                message: "Token muvaffaqiyatli yangilandi."
            );
        }

        // Sessiyani bekor qilish (Logout)
        public async Task<bool> RevokeSessionAsync(string sessionToken)
        {
            if (string.IsNullOrWhiteSpace(sessionToken))
                return false;

            var session = await _context.Sessions.FirstOrDefaultAsync(s => s.SessionToken == sessionToken && !s.IsRevoked);
            if (session == null)
                return false;

            session.IsRevoked = true;
            session.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserSessionDto?> GetSessionByTokenAsync(string sessionToken)
        {
            if (string.IsNullOrWhiteSpace(sessionToken))
                return null;

            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.SessionToken == sessionToken);

            if (session == null)
                return null;

            return new UserSessionDto
            {
                Id = session.Id,
                UserId = session.UserId,
                Username = session.User.Username,
                SessionToken = session.SessionToken,
                CreatedAt = session.CreatedAt,
                AccessTokenExpiresAt = session.AccessTokenExpiresAt,
                RefreshTokenExpiresAt = session.RefreshTokenExpiresAt,
                IsActive = session.IsActive,
                IsRevoked = session.IsRevoked,
                IpAddress = session.IpAddress,
                UserAgent = session.UserAgent
            };
        }

        public async Task<List<CafeDto>> GetAllCafesAsync()
        {
            return await _context.Cafes
                .Include(c => c.Owner)
                .Include(c => c.Invoices)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CafeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address,
                    PhoneNumber = c.PhoneNumber,
                    LogoUrl = c.LogoUrl,
                    OwnerId = c.OwnerId,
                    OwnerName = c.Owner.FullName ?? c.Owner.Username,
                    OwnerEmail = c.Owner.Email,
                    CreatedAt = c.CreatedAt,
                    ActiveUntil = c.ActiveUntil,
                    BasePrice = c.BasePrice,
                    IsSuspended = c.IsSuspended,
                    PendingInvoicesCount = c.Invoices.Count(i => i.Status == InvoiceStatus.Pending)
                })
                .ToListAsync();
        }

        public async Task<List<UserSessionDto>> GetUserSessionsAsync(int userId)
        {
            return await _context.Sessions
                .Include(s => s.User)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new UserSessionDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    Username = s.User.Username,
                    SessionToken = s.SessionToken,
                    CreatedAt = s.CreatedAt,
                    AccessTokenExpiresAt = s.AccessTokenExpiresAt,
                    RefreshTokenExpiresAt = s.RefreshTokenExpiresAt,
                    IsActive = s.IsActive,
                    IsRevoked = s.IsRevoked,
                    IpAddress = s.IpAddress,
                    UserAgent = s.UserAgent
                })
                .ToListAsync();
        }

        public async Task<List<UserSessionDto>> GetAllActiveSessionsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.Sessions
                .Include(s => s.User)
                .Where(s => !s.IsRevoked && s.RefreshTokenExpiresAt >= now)
                .OrderByDescending(s => s.CreatedAt)
                .Select(s => new UserSessionDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    Username = s.User.Username,
                    SessionToken = s.SessionToken,
                    CreatedAt = s.CreatedAt,
                    AccessTokenExpiresAt = s.AccessTokenExpiresAt,
                    RefreshTokenExpiresAt = s.RefreshTokenExpiresAt,
                    IsActive = s.IsActive,
                    IsRevoked = s.IsRevoked,
                    IpAddress = s.IpAddress,
                    UserAgent = s.UserAgent
                })
                .ToListAsync();
        }

        // Tizimda admin yo'q bo'lsa dastlabki admin yaratish
        public async Task EnsureSeedAdminAsync()
        {
            if (!await _context.Users.AnyAsync(u => u.Role == Role.Admin))
            {
                var admin = new User
                {
                    Username = "admin",
                    Email = "admin@sodiqcafe.uz",
                    FullName = "Bosh Administrator",
                    PhoneNumber = "+998901234567",
                    Role = Role.Admin,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(admin);
                await _context.SaveChangesAsync();
            }
        }
    }
}
