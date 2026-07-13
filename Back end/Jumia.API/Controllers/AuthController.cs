using System.Security.Claims;
using Jumia.DTOs;
using Jumia.Jumia.Domain.Models;
using Jumia.Jumia.Infrastructure;
using Jumia.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Jumia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IEmailService emailService,
            AppDbContext context,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _context = context;
            _config = config;
        }

        // ── Register ─────────────────────────────────────────────────────────────

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userExists = await _userManager.FindByEmailAsync(registerDto.Email);
            if (userExists != null)
                return BadRequest(new { Message = "Email is already registered" });

            var user = new AppUser
            {
                DisplayName = registerDto.DisplayName,
                Email = registerDto.Email,
                UserName = registerDto.Email.Split('@')[0]
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Message = "Registration failed", Errors = errors });
            }

            // Welcome email — fire-and-forget, does NOT block the response
            _ = _emailService.SendWelcomeEmailAsync(user.Email!, user.DisplayName);

            return Ok(new
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = _tokenService.CreateToken(user)
            });
        }

        // ── Login (Step 1 of 2FA) ─────────────────────────────────────────────────

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // 1. Validate credentials
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized(new { Message = "Invalid email or password" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { Message = "Invalid email or password" });

            // 2. Generate 6-digit OTP, removing any previous unused codes
            var existing = await _context.TwoFactorCodes
                .Where(c => c.UserId == user.Id && !c.IsUsed)
                .ToListAsync();
            _context.TwoFactorCodes.RemoveRange(existing);

            var code = Random.Shared.Next(100000, 999999).ToString();
            _context.TwoFactorCodes.Add(new TwoFactorCode
            {
                UserId = user.Id,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            });
            await _context.SaveChangesAsync();

            // 3. Send OTP (required — throws if email fails)
            await _emailService.SendOtpEmailAsync(user.Email!, user.DisplayName, code);

            return Ok(new
            {
                RequiresTwoFactor = true,
                Email = user.Email,
                Message = "A 6-digit verification code has been sent to your email."
            });
        }

        // ── Verify 2FA OTP (Step 2 — returns JWT) ────────────────────────────────

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new { Message = "Invalid request." });

            // Find a valid, non-expired, unused OTP
            var twoFactorCode = await _context.TwoFactorCodes
                .Where(c => c.UserId == user.Id
                         && c.Code == dto.Code
                         && !c.IsUsed
                         && c.ExpiresAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (twoFactorCode == null)
                return BadRequest(new { Message = "Invalid or expired verification code. Please request a new one." });

            // Mark as used
            twoFactorCode.IsUsed = true;
            await _context.SaveChangesAsync();

            // Login notification — fire-and-forget
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var loginTime = DateTime.UtcNow.ToString("dd MMM yyyy, hh:mm tt") + " UTC";
            _ = _emailService.SendLoginNotificationAsync(user.Email!, user.DisplayName, loginTime, ip);

            return Ok(new
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = _tokenService.CreateToken(user)
            });
        }

        // ── Resend 2FA OTP ────────────────────────────────────────────────────────

        [HttpPost("resend-2fa")]
        public async Task<IActionResult> ResendTwoFactor([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            // Security: always return OK to prevent email enumeration
            if (user == null)
                return Ok(new { Message = "If the email is registered, a new code has been sent." });

            var existing = await _context.TwoFactorCodes
                .Where(c => c.UserId == user.Id && !c.IsUsed)
                .ToListAsync();
            _context.TwoFactorCodes.RemoveRange(existing);

            var code = Random.Shared.Next(100000, 999999).ToString();
            _context.TwoFactorCodes.Add(new TwoFactorCode
            {
                UserId = user.Id,
                Code = code,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            });
            await _context.SaveChangesAsync();

            await _emailService.SendOtpEmailAsync(user.Email!, user.DisplayName, code);

            return Ok(new { Message = "A new verification code has been sent to your email." });
        }

        // ── Forgot Password ───────────────────────────────────────────────────────

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            // Security: always return OK
            if (user == null)
                return Ok(new { Message = "If the email is registered, a reset link has been sent." });

            // Generate Identity password-reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);
            var encodedEmail = Uri.EscapeDataString(dto.Email);

            var frontendUrl = _config["FrontendUrl"] ?? "http://jumiaapi.runasp.net";
            var resetLink = $"{frontendUrl}/reset-password.html?email={encodedEmail}&token={encodedToken}";

            await _emailService.SendPasswordResetAsync(user.Email!, user.DisplayName, resetLink);

            return Ok(new { Message = "If the email is registered, a reset link has been sent." });
        }

        // ── Reset Password ────────────────────────────────────────────────────────

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { Message = "Invalid request." });

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Message = "Password reset failed. The link may have expired.", Errors = errors });
            }

            return Ok(new { Message = "Password reset successfully. You can now log in with your new password." });
        }

        // ── Current User (requires JWT) ───────────────────────────────────────────

        [Authorize]
        [HttpGet("currentUser")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Unauthorized(new { Message = "Email claim is missing" });

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            return new UserDto
            {
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty,
                Token = _tokenService.CreateToken(user)
            };
        }
    }
}