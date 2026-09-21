using System.Collections.Generic;
using System.Threading.Tasks;
using SodiqCafeMVC.Application.DTOs;

namespace SodiqCafeMVC.Application.Interfaces
{
    public interface IAuthService
    {
        // Kafe egasini ro'yxatdan o'tkazish (Faqat Admin amalga oshiradi)
        Task<AuthResultDto> RegisterCafeOwnerAsync(RegisterOwnerDto dto, int adminUserId);

        // Mavjud foydalanuvchiga yangi kafe qo'shish
        Task<AuthResultDto> AddCafeToOwnerAsync(int ownerId, RegisterCafeDto dto, int adminUserId);

        // Tizimga kirish (Login) -> yangi sessiya ochiladi (tasodifiy token, access token, refresh token)
        Task<AuthResultDto> LoginAsync(LoginDto dto, string? ipAddress, string? userAgent);

        // Sessiyani yangilash (Refresh) -> session token va refresh token orqali
        Task<AuthResultDto> RefreshTokenAsync(string sessionToken, string refreshToken, string? ipAddress, string? userAgent);

        // Sessiyani bekor qilish (Logout)
        Task<bool> RevokeSessionAsync(string sessionToken);

        // Sessiya ma'lumoti
        Task<UserSessionDto?> GetSessionByTokenAsync(string sessionToken);

        // Barcha kafelar ro'yxati (Admin uchun)
        Task<List<CafeDto>> GetAllCafesAsync();

        // Foydalanuvchi sessiyalari
        Task<List<UserSessionDto>> GetUserSessionsAsync(int userId);

        // Barcha faol sessiyalar (Admin uchun)
        Task<List<UserSessionDto>> GetAllActiveSessionsAsync();

        // Dastlabki admin yo'q bo'lsa avtomatik yaratish
        Task EnsureSeedAdminAsync();
    }
}
