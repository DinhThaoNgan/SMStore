using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using CuaHangBanSach.Models;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CuaHangBanSach.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;

        public EmailSender(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient(_settings.Host)
            {
                Port = _settings.Port,
                Credentials = new NetworkCredential(_settings.Email, _settings.Password),
                EnableSsl = true
            };

            var message = new MailMessage(_settings.Email, email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };
            await client.SendMailAsync(message);
        }
    }
}