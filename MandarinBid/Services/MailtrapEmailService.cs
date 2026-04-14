using MandarinBid.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MandarinBid.Services
{
    public class MailtrapEmailService : IEmailService
    {
        // конфиг (для токена)
        private readonly IConfiguration _config;

        // http клиент
        private readonly HttpClient _http;

        // логгер
        private readonly ILogger<MailtrapEmailService> _logger;

        public MailtrapEmailService(
            IConfiguration config,
            HttpClient http,
            ILogger<MailtrapEmailService> logger)
        {
            _config = config;
            _http = http;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            try
            {
                // берём api токен из конфигурации
                var token = _config["EmailApi:Token"];

                // если токена нет — логируем и выходим
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Email sending failed: Mailtrap token is missing in configuration");
                    return;
                }

                // формируем payload для mailtrap api
                var payload = new
                {
                    from = new { email = "mailtrap@demomailtrap.co", name = "Mandarin Auction" },
                    to = new[] { new { email = to } },
                    subject = subject,
                    text = body
                };

                var json = JsonSerializer.Serialize(payload);

                // retry механизм (3 попытки)
                for (int i = 0; i < 3; i++)
                {
                    using var request = new HttpRequestMessage(HttpMethod.Post, "https://send.api.mailtrap.io/api/send");

                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);

                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _http.SendAsync(request);

                    // успех
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Email successfully sent to {Email}", to);
                        return;
                    }

                    // лог ошибки ответа
                    var result = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Email API attempt {Attempt} failed: {StatusCode}. Response: {Response}",
                        i + 1, response.StatusCode, result);

                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                // глобальная ошибка отправки
                _logger.LogError(ex, "Email sending failed to {Email}", to);
            }
        }
    }
}