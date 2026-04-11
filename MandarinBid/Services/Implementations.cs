using MandarinBid.Data;
using MandarinBid.Models;
using MandarinBid.Services.Background;
using MandarinBid.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MandarinBid.Services.Implementations
{
    public class AuctionService : IAuctionService
    {
        private readonly ApplicationDbContext _db;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IEmailService _email;

        public AuctionService(
            ApplicationDbContext db,
            IBackgroundTaskQueue queue,
            IEmailService email)
        {
            _db = db;
            _queue = queue;
            _email = email;
        }

        public async Task<List<Mandarin>> GetActiveMandarinsAsync()
        {
            return await _db.Mandarins
                .Include(m => m.Bids)
                .Where(m => m.ExpirationDate > DateTimeOffset.UtcNow)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<(bool Success, string Error)> PlaceBidAsync(int mandarinId, decimal amount, string userId)
        {
            var mandarin = await _db.Mandarins
                .Include(m => m.Bids)
                .FirstOrDefaultAsync(m => m.Id == mandarinId);

            if (mandarin == null)
                return (false, "Лот не найден");

            if (mandarin.ExpirationDate <= DateTimeOffset.UtcNow)
                return (false, "Аукцион уже завершён");

            var previousTopBid = mandarin.Bids
                .OrderByDescending(b => b.Amount)
                .FirstOrDefault();

            if (amount <= mandarin.CurrentPrice)
                return (false, "Ставка должна быть выше текущей");

            if (previousTopBid != null && previousTopBid.UserId == userId)
                return (false, "Ты уже лидер ставки");

            mandarin.CurrentPrice = amount;

            _db.Bids.Add(new Bid
            {
                MandarinId = mandarinId,
                Amount = amount,
                UserId = userId,
                CreatedAt = DateTimeOffset.UtcNow
            });

            try
            {
                await _db.SaveChangesAsync();

                if (previousTopBid != null)
                {
                    _queue.Queue(async token =>
                    {
                        var email = previousTopBid.UserId + "@test.com";

                        await _email.SendAsync(
                            email,
                            "Ваша ставка перебита",
                            $"Вашу ставку на {mandarin.Name} перебили"
                        );
                    });
                }

                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                return (false, "Ставку уже перебили, попробуй ещё раз");
            }
        }
    }
}