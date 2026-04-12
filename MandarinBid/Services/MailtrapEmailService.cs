using MandarinBid.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace MandarinBid.Services
{
    public class MailtrapEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public MailtrapEmailService(IConfiguration config)
        {
            _config = config;
            _http = new HttpClient();
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            try
            {
                var token = _config["EmailApi:Token"];

                var payload = new
                {
                    from = new { email = "mandarin@auction.com", name = "Mandarin Auction" },
                    to = new[] { new { email = to } },
                    subject = subject,
                    html = body   // ← ВОТ ЭТО КЛЮЧЕВОЕ
                };

                var json = JsonSerializer.Serialize(payload);

                var request = new HttpRequestMessage(HttpMethod.Post, "https://send.api.mailtrap.io/api/send");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _http.SendAsync(request);

                var result = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[EMAIL API] {response.StatusCode}");
                Console.WriteLine($"[EMAIL BODY] {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL API ERROR] {ex.Message}");
            }
        }
    }
}