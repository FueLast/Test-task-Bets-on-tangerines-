using MandarinBid.Data;
using MandarinBid.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections;

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
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
            var queue = scope.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();


            var expired = await db.Mandarins
                .Include(m => m.Bids)
                .Where(m => m.ExpirationDate <= DateTimeOffset.UtcNow)
                .ToListAsync();

            foreach (var mandarin in expired)
            {
                var winner = mandarin.Bids
                    .OrderByDescending(b => b.Amount)
                    .FirstOrDefault();

                if (winner != null)
                {
                    var email = winner.UserId + "@test.com";

                    queue.Queue(async token =>
                    {
                        await emailService.SendAsync(
                            email,
                            "Вы выиграли аукцион",
                            $"Вы выиграли {mandarin.Name} со ставкой {winner.Amount}"
                        );
                    });
                }
            }

            db.Mandarins.RemoveRange(expired);
            Console.WriteLine($"[Cleanup] удалено {expired.Count} мандаринок");
            await db.SaveChangesAsync();

        }
    }
}