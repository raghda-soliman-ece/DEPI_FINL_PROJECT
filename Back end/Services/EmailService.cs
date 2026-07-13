using System.Net;
using System.Net.Mail;

namespace Jumia.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private SmtpClient CreateSmtpClient()
        {
            var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var senderEmail = _config["Email:SenderEmail"] ?? string.Empty;
            var password = _config["Email:Password"] ?? string.Empty;

            return new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(senderEmail, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
        }

        private MailMessage CreateMessage(string toEmail, string toName, string subject)
        {
            var senderEmail = _config["Email:SenderEmail"] ?? "noreply@jumia.com";
            var senderName = _config["Email:SenderName"] ?? "Jumia Egypt";

            var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(toEmail, toName));
            return message;
        }

        // Shared email wrapper — wraps all HTML bodies in a branded container
        private static string WrapInLayout(string bodyHtml)
            => $@"
<div style='font-family: Arial, sans-serif; direction: rtl; max-width: 600px; margin: 0 auto; background: #f5f5f5; padding: 20px;'>
  <div style='background: #f68b1e; padding: 20px; text-align: center; border-radius: 10px 10px 0 0;'>
    <h1 style='color: white; margin: 0; font-size: 30px; letter-spacing: 2px;'>jumia</h1>
    <p style='color: rgba(255,255,255,0.85); margin: 4px 0 0; font-size: 13px;'>جوميا مصر</p>
  </div>
  <div style='background: white; padding: 30px; border-radius: 0 0 10px 10px; box-shadow: 0 4px 15px rgba(0,0,0,0.08);'>
    {bodyHtml}
    <hr style='border: none; border-top: 1px solid #eee; margin: 25px 0 15px;'>
    <p style='color: #bbb; font-size: 11px; text-align: center; margin: 0;'>© 2024 جوميا مصر — جميع الحقوق محفوظة</p>
  </div>
</div>";

        // ── Email Implementations ────────────────────────────────────────────────

        public async Task SendOtpEmailAsync(string toEmail, string displayName, string code)
        {
            try
            {
                // MOCK MODE: If no password is provided, just log the OTP to the console
                if (string.IsNullOrEmpty(_config["Email:Password"]))
                {
                    _logger.LogWarning("\n\n====== MOCK EMAIL ======\nTo: {Email}\nType: 2FA OTP\nCODE: {Code}\n========================\n", toEmail, code);
                    return;
                }

                using var message = CreateMessage(toEmail, displayName, "رمز التحقق الخاص بك — جوميا مصر");
                message.Body = WrapInLayout($@"
<h2 style='color: #333; margin: 0 0 8px;'>مرحباً {displayName}! 👋</h2>
<p style='color: #666; font-size: 15px; line-height: 1.7;'>رمز التحقق بخطوتين الخاص بك هو:</p>
<div style='background: #fff8f0; border: 2px dashed #f68b1e; border-radius: 12px; padding: 24px; text-align: center; margin: 22px 0;'>
  <span style='font-size: 46px; font-weight: 900; letter-spacing: 12px; color: #f68b1e; font-family: monospace;'>{code}</span>
</div>
<p style='color: #666; font-size: 14px;'>⏱️ هذا الرمز صالح لمدة <strong>10 دقائق</strong> فقط.</p>
<p style='color: #999; font-size: 13px;'>إذا لم تطلب هذا الرمز، يرجى تجاهل هذا البريد الإلكتروني فوراً.</p>");

                using var smtp = CreateSmtpClient();
                await smtp.SendMailAsync(message);
                _logger.LogInformation("OTP email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
                throw; // Caller should handle — 2FA cannot proceed without OTP
            }
        }

        public async Task SendLoginNotificationAsync(string toEmail, string displayName, string loginTime, string ipAddress)
        {
            try
            {
                if (string.IsNullOrEmpty(_config["Email:Password"]))
                {
                    _logger.LogInformation("MOCK EMAIL: Login notification for {Email} at {Time}", toEmail, loginTime);
                    return;
                }

                using var message = CreateMessage(toEmail, displayName, "تنبيه: تم تسجيل الدخول لحسابك — جوميا مصر");
                message.Body = WrapInLayout($@"
<h2 style='color: #333; margin: 0 0 8px;'>مرحباً {displayName}! 🔐</h2>
<p style='color: #666; font-size: 15px; line-height: 1.7;'>تم تسجيل الدخول إلى حسابك في جوميا مصر بنجاح.</p>
<div style='background: #e8f5e9; border-right: 4px solid #4caf50; padding: 16px 20px; margin: 22px 0; border-radius: 6px;'>
  <p style='margin: 0; color: #333; font-size: 14px;'>📅 <strong>التوقيت:</strong> {loginTime}</p>
  <p style='margin: 8px 0 0; color: #333; font-size: 14px;'>🌐 <strong>عنوان IP:</strong> {ipAddress}</p>
</div>
<p style='color: #666; font-size: 14px;'>إذا لم تكن أنت من قام بتسجيل الدخول، يُرجى <strong>تغيير كلمة المرور فوراً</strong> والتواصل معنا.</p>");

                using var smtp = CreateSmtpClient();
                await smtp.SendMailAsync(message);
                _logger.LogInformation("Login notification sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                // Fire-and-forget — notification failure must NOT block the login response
                _logger.LogWarning(ex, "Failed to send login notification to {Email}", toEmail);
            }
        }

        public async Task SendPasswordResetAsync(string toEmail, string displayName, string resetLink)
        {
            try
            {
                if (string.IsNullOrEmpty(_config["Email:Password"]))
                {
                    _logger.LogWarning("\n\n====== MOCK EMAIL ======\nTo: {Email}\nType: Password Reset\nLINK: {Link}\n========================\n", toEmail, resetLink);
                    return;
                }

                using var message = CreateMessage(toEmail, displayName, "إعادة تعيين كلمة المرور — جوميا مصر");
                message.Body = WrapInLayout($@"
<h2 style='color: #333; margin: 0 0 8px;'>مرحباً {displayName}! 🔑</h2>
<p style='color: #666; font-size: 15px; line-height: 1.7;'>تلقينا طلباً لإعادة تعيين كلمة المرور لحسابك على جوميا مصر.</p>
<div style='text-align: center; margin: 30px 0;'>
  <a href='{resetLink}'
     style='background: #f68b1e; color: white; padding: 15px 40px; border-radius: 8px;
            text-decoration: none; font-size: 16px; font-weight: 700; display: inline-block;'>
    إعادة تعيين كلمة المرور
  </a>
</div>
<p style='color: #999; font-size: 13px;'>⏱️ هذا الرابط صالح لمدة <strong>24 ساعة</strong> فقط.</p>
<p style='color: #999; font-size: 13px;'>إذا لم تطلب إعادة تعيين كلمة المرور، يرجى تجاهل هذا البريد الإلكتروني بأمان تام.</p>
<p style='color: #bbb; font-size: 12px; word-break: break-all;'>أو انسخ الرابط: {resetLink}</p>");

                using var smtp = CreateSmtpClient();
                await smtp.SendMailAsync(message);
                _logger.LogInformation("Password reset email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string displayName)
        {
            try
            {
                if (string.IsNullOrEmpty(_config["Email:Password"]))
                {
                    _logger.LogInformation("MOCK EMAIL: Welcome email for {Email}", toEmail);
                    return;
                }

                using var message = CreateMessage(toEmail, displayName, "أهلاً بك في جوميا مصر! 🎉");
                message.Body = WrapInLayout($@"
<h2 style='color: #333; margin: 0 0 8px;'>أهلاً وسهلاً {displayName}! 🎉</h2>
<p style='color: #666; font-size: 15px; line-height: 1.7;'>مبروك! تم إنشاء حسابك في جوميا مصر بنجاح.</p>
<p style='color: #666; font-size: 14px;'>يمكنك الآن التسوق من آلاف المنتجات بأفضل الأسعار مع توصيل سريع لباب بيتك.</p>
<div style='text-align: center; margin: 30px 0;'>
  <a href='http://jumiaapi.runasp.net'
     style='background: #f68b1e; color: white; padding: 15px 40px; border-radius: 8px;
            text-decoration: none; font-size: 16px; font-weight: 700; display: inline-block;'>
    ابدأ التسوق الآن 🛒
  </a>
</div>");

                using var smtp = CreateSmtpClient();
                await smtp.SendMailAsync(message);
                _logger.LogInformation("Welcome email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                // Fire-and-forget — registration should not fail if welcome email fails
                _logger.LogWarning(ex, "Failed to send welcome email to {Email}", toEmail);
            }
        }
    }
}
