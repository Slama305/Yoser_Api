using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yoser_API.Data;
using Yoser_API.Data.Models;
using Yoser_API.DTOs;

namespace Yoser_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // يتطلب تسجيل دخول
    public class PatientController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PatientController(AppDbContext context)
        {
            _context = context;
        }

        // 1. الحصول على بيانات البروفايل الخاص بي
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            // جلب الـ ID الخاص بالمستخدم من التوكن
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // جلب البروفايل مع بيانات اليوزر الأساسية (FullName, Email)
            var profile = await _context.PatientProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return NotFound("لم يتم العثور على بروفايل لهذا المستخدم.");

            // عرض البيانات بشكل منظم
            var response = new
            {
                profile.Id,
                profile.User.FullName,
                profile.User.Email,
                profile.Age,
                profile.MedicalCondition,
                profile.EmergencyContact,
                JoinedAt = profile.User.CreatedAt
            };

            return Ok(response);
        }

        // 2. تحديث بيانات البروفايل (مثل السن والحالة الصحية)
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdatePatientProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return BadRequest("البروفايل غير موجود.");

            // تحديث الحقول فقط
            profile.Age = dto.Age;
            profile.MedicalCondition = dto.MedicalCondition;
            profile.EmergencyContact = dto.EmergencyContact;

            _context.PatientProfiles.Update(profile);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم تحديث البيانات الطبية بنجاح." });
        }
    }
}