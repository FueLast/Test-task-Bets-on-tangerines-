using MandarinBid.Data;
using Microsoft.EntityFrameworkCore;

namespace MandarinBid.Services.Background
{
    public class MandarinCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public MandarinCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanupAsync();

                // ждем 24 часа
                //await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CleanupAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var expired = await db.Mandarins
                .Where(m => m.ExpirationDate <= DateTime.UtcNow)
                .ToListAsync();

            if (expired.Any())
            {
                db.Mandarins.RemoveRange(expired);
                await db.SaveChangesAsync();
                Console.WriteLine($"[Cleanup] удалено {expired.Count} мандаринок");
            }
        }
    }
}