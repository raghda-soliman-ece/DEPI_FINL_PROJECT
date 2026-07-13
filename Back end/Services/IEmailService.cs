namespace Jumia.Services
{
    public interface IEmailService
    {
        /// <summary>Sends a 6-digit OTP code for two-factor authentication.</summary>
        Task SendOtpEmailAsync(string toEmail, string displayName, string code);

        /// <summary>Notifies the user of a new login to their account.</summary>
        Task SendLoginNotificationAsync(string toEmail, string displayName, string loginTime, string ipAddress);

        /// <summary>Sends a password-reset link to the user's email.</summary>
        Task SendPasswordResetAsync(string toEmail, string displayName, string resetLink);

        /// <summary>Sends a welcome email after successful registration.</summary>
        Task SendWelcomeEmailAsync(string toEmail, string displayName);
    }
}
