using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yoser_API.Data.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; } // المريض اللي حجز

        [Required]
        public int ProviderId { get; set; } // الدكتور أو الممرض اللي اتحجز عنده

        [Required]
        public DateTime AppointmentDate { get; set; } // ميعاد الحجز

        [MaxLength(500)]
        public string Notes { get; set; } // ملاحظات المريض (اختياري)

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled

        [ForeignKey("PatientId")]
        public virtual PatientProfile Patient { get; set; }

        [ForeignKey("ProviderId")]
        public virtual MedicalProvider Provider { get; set; }
    }
}