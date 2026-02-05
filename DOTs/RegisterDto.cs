using System.ComponentModel.DataAnnotations;
using Yoser_API.Data.Models; // عشان يشوف الـ UserType Enum

namespace Yoser_API.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "الأسم بالكامل مطلوب")]
        public string FullName { get; set; }

        [Required, EmailAddress(ErrorMessage = "صيغة الإيميل غير صحيحة")]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "الباسورد لازم يكون أكتر من 6 حروف")]
        public string Password { get; set; }

        [Required]
        public UserType RoleType { get; set; } // (Senior, Determination, Provider)
    }
}
