using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yoser_API.Data.Models
{
    public class MedicationReminder
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; } // FK لملف المريض

        [Required, MaxLength(100)]
        public string MedName { get; set; }

        [Required, MaxLength(50)]
        public string Dosage { get; set; }

        [Required]
        public DateTime ReminderTime { get; set; }

        public bool IsTaken { get; set; } = false;

        [ForeignKey("PatientId")]
        public virtual PatientProfile Patient { get; set; }
    }

}
