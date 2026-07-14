using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Jumia.DTOs;
using Jumia.Jumia.Domain.Models;
using Jumia.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        private readonly ITwoFactorService _twoFactorService;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IEmailService emailService,
            ITwoFactorService twoFactorService,
            IConfiguration config,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailService = emailService;
            _twoFactorService = twoFactorService;
            _config = config;
            _logger = logger;
        }

        private bool RequireTwoFactor => _config.GetValue("AuthSettings:RequireTwoFactor", false);

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

            if (!RequireTwoFactor)
            {
                return Ok(new
                {
                    displayName = user.DisplayName,
                    email = user.Email,
                    token = _tokenService.CreateToken(user)
                });
            }

            return await ChallengeTwoFactorAsync(user, "Registration successful.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized(new { Message = "Invalid email or password" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { Message = "Invalid email or password" });

            if (!RequireTwoFactor)
            {
                return Ok(new
                {
                    displayName = user.DisplayName,
                    email = user.Email,
                    token = _tokenService.CreateToken(user)
                });
            }

            return await ChallengeTwoFactorAsync(user, "Password verified.");
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyTwoFactorDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Code))
                return BadRequest(new { Message = "Email and code are required" });

            if (dto.Code.Trim().Length != 6)
                return BadRequest(new { Message = "Code must be 6 digits" });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { Message = "Invalid or expired verification code. Please request a new one." });

            if (!_twoFactorService.VerifyCode(dto.Email, dto.Code))
                return BadRequest(new { Message = "Invalid or expired verification code. Please request a new one." });

            return Ok(new
            {
                displayName = user.DisplayName,
                email = user.Email,
                token = _tokenService.CreateToken(user)
            });
        }

        [HttpPost("resend-2fa")]
        public async Task<IActionResult> ResendTwoFactor([FromBody] ForgotPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { Message = "Email is required" });

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                // Don't leak whether the email exists
                return Ok(new { Message = "If the account exists, a new verification code has been sent." });
            }

            return await ChallengeTwoFactorAsync(user, "A new verification code has been sent to your email.");
        }

        [Authorize]
        [HttpGet("currentUser")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email!);
            if (user == null) return Unauthorized();

            return new UserDto
            {
                DisplayName = user.DisplayName,
                Email = user.Email!,
                Token = _tokenService.CreateToken(user)
            };
        }

        private async Task<IActionResult> ChallengeTwoFactorAsync(AppUser user, string messagePrefix)
        {
            var code = _twoFactorService.CreateCode(user.Email!);
            var subject = "Jumia verification code";
            var body = $"<p>Your Jumia verification code is:</p><h2>{code}</h2><p>It expires in 10 minutes.</p>";

            var (sent, error) = await _emailService.SendAsync(user.Email!, subject, body);
            _logger.LogInformation(
                "2FA for {Email}: emailSent={Sent}, error={Error}, code={Code}",
                user.Email, sent, error, code);

            // If SMTP is missing/broken, still return the code so auth is usable for demos.
            var exposeCode = !sent || _config.GetValue("AuthSettings:ExposeCodeWhenEmailFails", true);

            return Ok(new
            {
                requiresTwoFactor = true,
                displayName = user.DisplayName,
                email = user.Email,
                message = sent
                    ? $"{messagePrefix} A verification code has been sent to your email."
                    : $"{messagePrefix} Email delivery failed ({error}). Use the verification code shown in the app.",
                verificationCode = exposeCode ? code : null
            });
        }
    }
}
