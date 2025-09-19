using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Cinema.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpHost = _configuration["Smtp:Host"];
            var smtpPort = int.Parse(_configuration["Smtp:Port"]);
            var smtpFrom = _configuration["Smtp:From"];
            var smtpTo = _configuration["Smtp:To"];
            var smtpUser = _configuration["Smtp:UserName"];
            var smtpPass = _configuration["Smtp:Password"];

            // Validate that From and UserName are not empty
            if (string.IsNullOrWhiteSpace(smtpUser))
                throw new ArgumentException("SMTP username is not set in configuration.");
            if (string.IsNullOrWhiteSpace(smtpFrom))
                throw new ArgumentException("SMTP From address is not set in configuration.");

            var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Smtp:From"], "Cinema"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }

}
