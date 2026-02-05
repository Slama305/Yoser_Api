using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Yoser_API.Data;
using Yoser_API.Data.Models;
using Yoser_API.DOTs;
using Yoser_API.DTOs;

namespace Yoser_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PharmacyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PharmacyController(AppDbContext context)
        {
            _context = context;
        }

        // 1. رفع طلب صيدلية جديد (تخزين في الداتابيز)
        [HttpPost("upload-prescription")]
        public async Task<IActionResult> UploadPrescription([FromForm] CreateOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null) return BadRequest("بروفايل المريض غير موجود.");

            if (dto.ImageFile == null || dto.ImageFile.Length == 0)
                return BadRequest("يرجى رفع صورة الروشتة.");

            // تحويل الصورة إلى Byte Array
            using var memoryStream = new MemoryStream();
            await dto.ImageFile.CopyToAsync(memoryStream);

            var order = new PharmacyOrder
            {
                PatientId = patient.Id,
                PrescriptionData = memoryStream.ToArray(),
                ImageContentType = dto.ImageFile.ContentType,
                Status = "Pending"
            };

            _context.PharmacyOrders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم رفع الطلب بنجاح وتخزين الصورة في قاعدة البيانات." });
        }

        // 2. الحصول على الصورة لعرضها في الفرونت إند
        [HttpGet("image/{orderId}")]
        public async Task<IActionResult> GetPrescriptionImage(int orderId)
        {
            var order = await _context.PharmacyOrders.FindAsync(orderId);

            if (order == null || order.PrescriptionData == null)
                return NotFound("الصورة غير موجودة.");

            // إرجاع مصفوفة البايتات كملف صورة
            return File(order.PrescriptionData, order.ImageContentType ?? "image/jpeg");
        }

        // 3. عرض جميع طلباتي كمريض
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _context.PharmacyOrders
                .Where(o => o.Patient.UserId == userId)
                .Select(o => new { o.Id, o.Status })
                .ToListAsync();

            return Ok(orders);
        }
    }
}