using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Yoser_API.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        public UserType RoleType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
