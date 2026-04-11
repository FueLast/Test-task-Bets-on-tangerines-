using System.Net;
using System.Net.Mail;
using MandarinBid.Services.Interfaces;

namespace MandarinBid.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("your@email.com", "password"),
                EnableSsl = true
            };

            var mail = new MailMessage("your@email.com", to, subject, body);

            await client.SendMailAsync(mail);
        }
    }
}
