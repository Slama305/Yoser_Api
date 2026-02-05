using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yoser_API.Data.Models;

public class PharmacyOrder
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Required]
    public byte[] PrescriptionData { get; set; } // تخزين الصورة كبيانات ثنائية

    public string? ImageContentType { get; set; } // لتخزين نوع الصورة (image/png, image/jpeg)

    [Required, MaxLength(50)]
    public string Status { get; set; } = "Pending";

    [ForeignKey("PatientId")]
    public virtual PatientProfile Patient { get; set; }
}