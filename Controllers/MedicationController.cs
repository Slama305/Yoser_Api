using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yoser_API.Data;
using Yoser_API.Data.Models;
using Yoser_API.DOTs;

namespace Yoser_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // حماية كاملة للكنترولر - يتطلب JWT Token
    public class MedicationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MedicationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddMedication(MedicationRequestDto dto) // تغيير هنا
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return BadRequest("لم يتم العثور على بروفايل طبي لهذا المستخدم.");

            // تحويل الـ DTO لـ Entity يدوياً
            var medication = new MedicationReminder
            {
                MedName = dto.MedName,
                Dosage = dto.Dosage,
                ReminderTime = dto.ReminderTime,
                PatientId = profile.Id,
                IsTaken = false
            };

            _context.MedicationReminders.Add(medication);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم إضافة موعد الدواء بنجاح", Data = medication });
        }

        // 2. عرض كل أدوية المستخدم الحالي فقط
        [HttpGet("my-medications")]
        public async Task<IActionResult> GetMyMedications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null) return NotFound("البروفايل غير موجود.");

            var medications = await _context.MedicationReminders
                .Where(m => m.PatientId == profile.Id)
                .OrderBy(m => m.ReminderTime)
                .ToListAsync();

            return Ok(medications);
        }

        // 3. تحديث حالة الدواء (تم أخذه)
        [HttpPut("mark-taken/{id}")]
        public async Task<IActionResult> MarkAsTaken(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var medication = await _context.MedicationReminders
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id && m.Patient.UserId == userId);

            if (medication == null)
                return NotFound("الدواء غير موجود أو لا تملك صلاحية الوصول إليه.");

            medication.IsTaken = true;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم تسجيل أخذ الجرعة بنجاح." });
        }

        // 4. تعديل بيانات دواء موجود
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateMedication(int id, MedicationReminder updatedMed)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingMed = await _context.MedicationReminders
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id && m.Patient.UserId == userId);

            if (existingMed == null) return NotFound();

            existingMed.MedName = updatedMed.MedName;
            existingMed.Dosage = updatedMed.Dosage;
            existingMed.ReminderTime = updatedMed.ReminderTime;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "تم تحديث البيانات بنجاح." });
        }

        // 5. حذف دواء من الجدول
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMedication(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var medication = await _context.MedicationReminders
                .Include(m => m.Patient)
                .FirstOrDefaultAsync(m => m.Id == id && m.Patient.UserId == userId);

            if (medication == null) return NotFound();

            _context.MedicationReminders.Remove(medication);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم حذف الدواء من جدول المواعيد." });
        }
    }
}
