using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yoser_API.Data.Models
{
    public class MedicalProvider
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required, MaxLength(100)]
        public string Specialty { get; set; }

        [MaxLength(1000)]
        public string Bio { get; set; }

        [Required, MaxLength(200)]
        public string Address { get; set; }

        [Range(0, 10000)]
        public decimal Price { get; set; }

        public bool IsAvailable { get; set; } = true;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}