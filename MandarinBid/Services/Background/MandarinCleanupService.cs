using MandarinBid.Data;
using MandarinBid.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MandarinBid.Services.Background
{
    // фоновый сервис для обработки завершённых аукционов
    public class MandarinCleanupService : BackgroundService
    {
        // фабрика scope — нужна, потому что background service живёт дольше запроса
        private readonly IServiceScopeFactory _scopeFactory;

        // логгер для отслеживания работы сервиса
        private readonly ILogger<MandarinCleanupService> _logger;

        public MandarinCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<MandarinCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        /// <summary>
        /// периодически запускает очистку завершённых аукционов
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanupAsync();

                // в проде обычно раз в сутки, здесь уменьшено для тестирования
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        /// <summary>
        /// обрабатывает завершённые аукционы:
        /// - определяет победителя
        /// - отправляет уведомление
        /// - удаляет лот
        /// </summary>
        /// <remarks>
        /// использует отдельный scope для работы с scoped сервисами
        /// </remarks>
        private async Task CleanupAsync()
        {
            try
            {
                // создаём scope для получения scoped-сервисов (db, userManager и т.д.)
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var queue = scope.ServiceProvider.GetRequiredService<IBackgroundTaskQueue>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

                var now = DateTimeOffset.UtcNow;

                // находим завершённые и ещё не обработанные лоты
                var expired = await db.Mandarins
                    .Include(m => m.Bids)
                    .Where(m => m.ExpirationDate <= now.AddSeconds(-5) && !m.IsProcessed)
                    .ToListAsync();

                _logger.LogInformation("Processing expired mandarins: {Count}", expired.Count);

                foreach (var mandarin in expired)
                {
                    // определяем победителя (максимальная ставка)
                    var winner = mandarin.Bids
                        .OrderByDescending(b => b.Amount)
                        .FirstOrDefault();

                    // если ставок не было — просто помечаем как обработанный
                    if (winner == null)
                    {
                        mandarin.IsProcessed = true;

                        _logger.LogInformation("Mandarin {Name} finished without bids", mandarin.Name);
                        continue;
                    }

                    // получаем пользователя-победителя
                    var user = await userManager.FindByIdAsync(winner.UserId);

                    // если у пользователя есть email — отправляем уведомление
                    if (user != null && !string.IsNullOrEmpty(user.Email))
                    {
                        var email = user.Email;

                        _logger.LogInformation("Winner found: {User} for mandarin {Id}", user.UserName, mandarin.Id);

                        // добавляем отправку email в очередь (не блокируем поток)
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

                    // помечаем лот как обработанный, чтобы не обрабатывать повторно
                    mandarin.IsProcessed = true;
                }

                // удаляем обработанные лоты (в реальном проекте лучше архивировать)
                db.Mandarins.RemoveRange(expired);

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // ловим любые ошибки, чтобы сервис не падал
                _logger.LogError(ex, "Cleanup failed");
            }
        }
    }
}