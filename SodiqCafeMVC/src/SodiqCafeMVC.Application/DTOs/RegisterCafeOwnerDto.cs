using System.ComponentModel.DataAnnotations;

namespace SodiqCafeMVC.Application.DTOs
{
    public class RegisterOwnerDto
    {
        [Required(ErrorMessage = "Foydalanuvchi nomi kiritilishi shart")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Foydalanuvchi nomi 3 dan 50 belgigacha bo'lishi kerak")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Elektron pochta kiritilishi shart")]
        [EmailAddress(ErrorMessage = "Elektron pochta formati noto'g'ri")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parol kiritilishi shart")]
        [MinLength(6, ErrorMessage = "Parol kamida 6 belgidan iborat bo'lishi kerak")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kafe egasining to'liq ismi kiritilishi shart")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefon raqam formati noto'g'ri")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Dastlabki balans (so'mda)")]
        [Range(0, 100000000, ErrorMessage = "Dastlabki balans 0 dan kam bo'lishi mumkin emas")]
        public decimal InitialBalance { get; set; } = 0m;
    }

    public class RegisterCafeDto
    {
        public int OwnerId { get; set; }

        [Required(ErrorMessage = "Kafe nomi kiritilishi shart")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Kafe nomi 2 dan 100 belgigacha bo'lishi kerak")]
        public string CafeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kafe manzili kiritilishi shart")]
        public string CafeAddress { get; set; } = string.Empty;

        public string? CafePhone { get; set; }

        [Display(Name = "Oylik to'lov summasi (so'mda)")]
        [Range(0, 100000000, ErrorMessage = "Oylik to'lov 0 dan kam bo'lishi mumkin emas")]
        public decimal BasePrice { get; set; } = 300000m;
    }
}
