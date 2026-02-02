using Microsoft.AspNetCore.Identity.UI.Services;

namespace EcoCity.Services
{
    public class NoOpEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // En développement, on écrit juste dans les logs
            Console.WriteLine($"Email envoyé à {email}: {subject}");
            return Task.CompletedTask;
        }
    }
}
