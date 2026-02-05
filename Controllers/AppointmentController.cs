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
    public class AppointmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        // 1. مريض يحجز ميعاد
        [HttpPost("book")]
        public async Task<IActionResult> Book(BookAppointmentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null) return BadRequest("يجب أن تكون مريضاً لتتمكن من الحجز.");

            var appointment = new Appointment
            {
                PatientId = patient.Id,
                ProviderId = dto.ProviderId,
                AppointmentDate = dto.AppointmentDate,
                Notes = dto.Notes,
                Status = "Pending"
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "تم إرسال طلب الحجز بنجاح وفي انتظار موافقة مقدم الخدمة." });
        }

        // 2. المريض يشوف حجوزاته
        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointments = await _context.Appointments
                .Include(a => a.Provider.User)
                .Where(a => a.Patient.UserId == userId)
                .ToListAsync();

            return Ok(appointments);
        }

        // 3. مقدم الخدمة يشوف الحجوزات اللي جاتله عشان يوافق أو يرفض
        [HttpGet("provider-requests")]
        public async Task<IActionResult> GetProviderRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var appointments = await _context.Appointments
                .Include(a => a.Patient.User)
                .Where(a => a.Provider.UserId == userId)
                .ToListAsync();

            return Ok(appointments);
        }
    }
}