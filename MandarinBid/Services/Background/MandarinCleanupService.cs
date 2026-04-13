using MandarinBid.Data;
using MandarinBid.Models;
using MandarinBid.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Reflection;

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
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var now = DateTimeOffset.UtcNow;

            var expired = await db.Mandarins
                .Include(m => m.Bids)
                .Where(m => m.ExpirationDate <= now.AddSeconds(-5) && !m.IsProcessed)
                .ToListAsync();

            foreach (var mandarin in expired)
            {
                var winner = mandarin.Bids
                    .OrderByDescending(b => b.Amount)
                    .FirstOrDefault();

                // нет ставок -просто пропускаем
                if (winner == null)
                {
                    mandarin.IsProcessed = true;
                    Console.WriteLine($"[Cleanup] {mandarin.Name} завершён без ставок");
                    continue;
                }

                var user = await userManager.FindByIdAsync(winner.UserId);

                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var email = user.Email;

                    queue.Queue(async token =>
                    {
                        var body = $@"
🍊 MANDARIN AUCTION

══════════════════════════════
ЧЕК № {mandarin.Id}
══════════════════════════════

Лот:        {mandarin.Name}
Победитель: {user.UserName}
Ставка:     {winner.Amount} ₽

Дата: {DateTimeOffset.UtcNow.ToLocalTime():dd.MM.yyyy HH:mm:ss}

══════════════════════════════
Спасибо за участие!
";

                        await emailService.SendAsync(
                            email,
                            "Вы выиграли аукцион 🍊",
                            body
                        );
                    });

                    mandarin.IsProcessed = true;

                }
            }

            db.Mandarins.RemoveRange(expired);
            Console.WriteLine($"[Cleanup] удалено {expired.Count} мандаринок");
            await db.SaveChangesAsync();

        }
    }
}