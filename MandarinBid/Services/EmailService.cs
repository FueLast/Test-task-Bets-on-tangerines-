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
            try
            {
                var client = new SmtpClient(_config["Email:Host"], int.Parse(_config["Email:Port"]))
                {
                    Credentials = new NetworkCredential(
                        _config["Email:User"],
                        _config["Email:Pass"]
                    ),
                    EnableSsl = true
                };

                var mail = new MailMessage(
                    _config["Email:From"],
                    to,
                    subject,
                    body
                );

                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] {ex.Message}");
            }
        }
    }
}
