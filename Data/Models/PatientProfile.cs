using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yoser_API.Data.Models
{
    public class PatientProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required, MaxLength(500)]
        public string MedicalCondition { get; set; }

        [Range(1, 120)]
        public int Age { get; set; }

        [Required, Phone]
        public string EmergencyContact { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
    }
}