using System.ComponentModel.DataAnnotations;

namespace SodiqCafeMVC.Application.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Foydalanuvchi nomi yoki elektron pochta kiritilishi shart")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Parol kiritilishi shart")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = true;
    }
}
