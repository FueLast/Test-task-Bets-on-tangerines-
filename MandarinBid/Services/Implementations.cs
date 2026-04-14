using MandarinBid.Data;
using MandarinBid.Models;
using MandarinBid.Services.Background;
using MandarinBid.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MandarinBid.Services.Implementations
{
    public class AuctionService : IAuctionService
    {
        // основной контекст базы данных
        private readonly ApplicationDbContext _db;

        // очередь фоновых задач (для email и прочего)
        private readonly IBackgroundTaskQueue _queue;

        // сервис отправки email
        private readonly IEmailService _email;

        // менеджер пользователей (identity)
        private readonly UserManager<IdentityUser> _userManager;

        // логгер
        private readonly ILogger<AuctionService> _logger;

        /// <summary>
        /// возвращает все активные лоты (у которых не истёк срок)
        /// </summary>
        /// <remarks>
        /// включает связанные ставки и использует asnotracking для оптимизации чтения
        /// </remarks>
        public AuctionService(
            ApplicationDbContext db,
            IBackgroundTaskQueue queue,
            IEmailService email,
            UserManager<IdentityUser> userManager,
            ILogger<AuctionService> logger)
        {
            _db = db;
            _queue = queue;
            _email = email;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// получает список активных (не завершённых) аукционов
        /// </summary>
        /// <returns>список мандаринов с их ставками</returns>
        public async Task<List<Mandarin>> GetActiveMandarinsAsync()
        {
            return await _db.Mandarins
                .Include(m => m.Bids) // подтягиваем ставки
                .Where(m => m.ExpirationDate > DateTimeOffset.UtcNow) // только активные
                .AsNoTracking() // оптимизация: только чтение
                .ToListAsync();
        }
        /// <summary>
        /// обрабатывает бизнес-логику размещения ставки
        /// </summary>
        /// <param name="mandarinId">id лота</param>
        /// <param name="amount">сумма ставки</param>
        /// <param name="userId">id пользователя</param>
        /// <returns>
        /// success + сообщение об ошибке (если есть)
        /// </returns>
        /// <remarks>
        /// выполняет:
        /// - валидацию ставки
        /// - защиту от самоперебивания
        /// - обновление текущей цены
        /// - создание записи ставки
        /// - отправку уведомления перебитому пользователю (async через очередь)
        /// </remarks>
        public async Task<(bool Success, string Error)> PlaceBidAsync(int mandarinId, decimal amount, string userId)
        {
            // загружаем мандарин вместе со ставками
            var mandarin = await _db.Mandarins
                .Include(m => m.Bids)
                .FirstOrDefaultAsync(m => m.Id == mandarinId);

            if (mandarin == null)
                return (false, "Лот не найден");

            // защита от ставок после окончания
            if (mandarin.ExpirationDate <= DateTimeOffset.UtcNow)
                return (false, "Аукцион уже завершён");

            // находим текущую топ ставку
            var previousTopBid = mandarin.Bids
                .OrderByDescending(b => b.Amount)
                .FirstOrDefault();

            // ставка должна быть выше текущей цены
            if (amount <= mandarin.CurrentPrice)
                return (false, "Ставка должна быть выше текущей");

            // нельзя перебивать самого себя
            if (previousTopBid != null && previousTopBid.UserId == userId)
                return (false, "Ты уже лидер ставки");

            // обновляем текущую цену
            mandarin.CurrentPrice = amount;

            // получаем пользователя для записи username
            var user = await _userManager.FindByIdAsync(userId);

            // добавляем новую ставку
            _db.Bids.Add(new Bid
            {
                MandarinId = mandarinId,
                Amount = amount,
                UserId = userId,
                UserName = user?.UserName ?? "unknown", // fallback если что-то пошло не так
                CreatedAt = DateTimeOffset.UtcNow
            });

            try
            {
                await _db.SaveChangesAsync();

                // лог успешной ставки
                _logger.LogInformation("New bid placed: {Amount} on {MandarinId}", amount, mandarin.Id);

                // если был предыдущий лидер — отправляем ему уведомление
                if (previousTopBid != null)
                {
                    _queue.Queue(async token =>
                    {
                        var user = await _userManager.FindByIdAsync(previousTopBid.UserId);

                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            var email = user.Email;

                            _logger.LogInformation("Outbid email queued for {Email}", email);

                            await _email.SendAsync(
                                email,
                                "Ваша ставка перебита",
                                $"Вашу ставку на {mandarin.Name} перебили"
                            );

                            _logger.LogInformation("Outbid email sent to {Email}", email);
                        }
                    });
                }

                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                // если произошёл конфликт (два пользователя одновременно ставят)
                return (false, "Ставку уже перебили, попробуй ещё раз");
            }
        }
    }
}