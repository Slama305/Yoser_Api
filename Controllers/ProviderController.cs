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
    [Authorize]
    public class ProviderController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProviderController(AppDbContext context)
        {
            _context = context;
        }

        // 1. إكمال أو تحديث بيانات الطبيب
        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProviderProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // البحث عن البروفايل أو إنشاؤه إذا لم يكن موجوداً
            var provider = await _context.MedicalProviders.FirstOrDefaultAsync(p => p.UserId == userId);

            if (provider == null)
            {
                provider = new MedicalProvider { UserId = userId };
                _context.MedicalProviders.Add(provider);
            }

            provider.Specialty = dto.Specialty;
            provider.Bio = dto.Bio;
            provider.Address = dto.Address;
            provider.Price = dto.Price;
            provider.IsAvailable = dto.IsAvailable;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "تم تحديث بياناتك المهنية بنجاح." });
        }

        // 2. الحصول على بيانات بروفايلي (للطبيب نفسه)
        [HttpGet("my-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var provider = await _context.MedicalProviders
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (provider == null) return NotFound("لم يتم العثور على بيانات مقدم الخدمة.");

            return Ok(provider);
        }

        // 3. عرض جميع الأطباء المتاحين (للمرضى)
        [HttpGet("all-providers")]
        [AllowAnonymous] // متاح للجميع حتى بدون تسجيل دخول
        public async Task<IActionResult> GetAllProviders([FromQuery] string? specialty)
        {
            var query = _context.MedicalProviders
                .Include(p => p.User)
                .Where(p => p.IsAvailable);

            // إمكانية الفلترة حسب التخصص
            if (!string.IsNullOrEmpty(specialty))
            {
                query = query.Where(p => p.Specialty.Contains(specialty));
            }

            var providers = await query.Select(p => new {
                p.Id,
                p.User.FullName,
                p.Specialty,
                p.Price,
                p.Address,
                p.Bio
            }).ToListAsync();

            return Ok(providers);
        }
    }
}