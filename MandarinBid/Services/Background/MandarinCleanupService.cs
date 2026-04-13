using MandarinBid.Data;
using MandarinBid.Models;
using MandarinBid.Services.Implementations;
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

        private readonly ILogger<MandarinCleanupService> _logger;

        public MandarinCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<MandarinCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
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

            _logger.LogInformation("Processing expired mandarins: {Count}", expired.Count);

            foreach (var mandarin in expired)
            {
                var winner = mandarin.Bids
                    .OrderByDescending(b => b.Amount)
                    .FirstOrDefault();

                if (winner == null)
                {
                    mandarin.IsProcessed = true;

                    _logger.LogInformation("Mandarin {Name} finished without bids", mandarin.Name);
                    continue;
                }

                var user = await userManager.FindByIdAsync(winner.UserId);

                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var email = user.Email;

                    _logger.LogInformation("Winner found: {User} for mandarin {Id}", user.UserName, mandarin.Id);

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
                        await emailService.SendAsync(email, "Вы выиграли аукцион 🍊", body);
                    });

                    _logger.LogInformation("Email queued for winner: {Email}", email);
                }

                mandarin.IsProcessed = true;
            }


            db.Mandarins.RemoveRange(expired);
            _logger.LogInformation("Deleted mandarins: {Count}", expired.Count);

            await db.SaveChangesAsync();
        }
    }
}