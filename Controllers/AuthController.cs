using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Yoser_API.Data;
using Yoser_API.Data.Models;
using Yoser_API.DTOs;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    private readonly string _jwtKey;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtDuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _context = context;

        _jwtKey = configuration["JWT:Key"]
            ?? throw new Exception("JWT:Key is missing");

        _jwtIssuer = configuration["JWT:Issuer"]
            ?? throw new Exception("JWT:Issuer is missing");

        _jwtAudience = configuration["JWT:Audience"]
            ?? throw new Exception("JWT:Audience is missing");

        _jwtDuration = int.Parse(
            configuration["JWT:DurationInDays"] ?? "1");
    }

    // ================= REGISTER =================

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        // 1. تعريف استراتيجية التنفيذ
        var strategy = _context.Database.CreateExecutionStrategy();

        try
        {
            // 2. تنفيذ العملية داخل الاستراتيجية لضمان توافقها مع الـ Retrying Strategy
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (await _userManager.FindByEmailAsync(dto.Email) != null)
                        return BadRequest("الإيميل مستخدم بالفعل");

                    var user = new ApplicationUser
                    {
                        UserName = dto.Email,
                        Email = dto.Email,
                        FullName = dto.FullName,
                        RoleType = dto.RoleType
                    };

                    var result = await _userManager.CreateAsync(user, dto.Password);

                    if (!result.Succeeded)
                        return BadRequest(result.Errors);

                    // إضافة Role للـ Identity
                    await _userManager.AddToRoleAsync(user, dto.RoleType.ToString());

                    if (dto.RoleType == UserType.Senior || dto.RoleType == UserType.Determination)
                    {
                        var profile = new PatientProfile
                        {
                            UserId = user.Id,
                            MedicalCondition = "Pending Setup",
                            EmergencyContact = "Not Provided",
                            Age = 0
                        };

                        _context.PatientProfiles.Add(profile);
                        await _context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return Ok(new { Message = "User created successfully" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, ex.Message);
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "حدث خطأ أثناء معالجة الطلب: " + ex.Message);
        }
    }

    // ================= LOGIN =================

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized("الإيميل أو كلمة السر غلط");

        var token = GenerateJwtToken(user);

        return Ok(new AuthResponseDto
        {
            IsAuthenticated = true,
            Message = "تم تسجيل الدخول",
            Token = token,
            ExpiresOn = DateTime.UtcNow.AddDays(_jwtDuration)
        });
    }

    // ================= JWT MACHINE =================

    private string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("FullName", user.FullName ?? string.Empty),
            new Claim(ClaimTypes.Role, user.RoleType.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jwtDuration),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
