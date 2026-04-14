using MandarinBid.Data;
using MandarinBid.Models;

namespace MandarinBid.Services
{
    public class MandarinGeneratorService : BackgroundService
    {
        // фабрика scope для работы с scoped сервисами (db)
        private readonly IServiceScopeFactory _scopeFactory;

        public MandarinGeneratorService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // бесконечный цикл пока приложение работает
            while (!stoppingToken.IsCancellationRequested)
            {
                await GenerateMandarin();

                // генерация раз в минуту
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task GenerateMandarin()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTimeOffset.UtcNow;

            // создаём новый лот
            var mandarin = new Mandarin
            {
                Name = $"🍊Мандаринка {now.ToLocalTime():HH:mm:ss}",
                CurrentPrice = 50,
                CreatedAt = now,
                ExpirationDate = now.AddMinutes(1) // короткий жизненный цикл
            };

            db.Mandarins.Add(mandarin);
            await db.SaveChangesAsync();
        }
    }
}