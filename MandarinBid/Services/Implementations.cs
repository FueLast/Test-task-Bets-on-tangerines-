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
        private readonly ApplicationDbContext _db;
        private readonly IBackgroundTaskQueue _queue;
        private readonly IEmailService _email;

        private readonly UserManager<IdentityUser> _userManager;

        public AuctionService(
            ApplicationDbContext db,
            IBackgroundTaskQueue queue,
            IEmailService email,
            UserManager<IdentityUser> userManager)
        {
            _db = db;
            _queue = queue;
            _email = email;
            _userManager = userManager;
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

            var user = await _userManager.FindByIdAsync(userId);

            _db.Bids.Add(new Bid
            {
                MandarinId = mandarinId,
                Amount = amount,
                UserId = userId,
                UserName = user?.UserName ?? "unknown",
                CreatedAt = DateTimeOffset.UtcNow
            });

            try
            {
                await _db.SaveChangesAsync();

                if (previousTopBid != null)
                {
                    _queue.Queue(async token =>
                    {
                        var user = await _userManager.FindByIdAsync(previousTopBid.UserId);

                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            var email = user.Email;

                            Console.WriteLine($"[EMAIL QUEUED] {email}");

                            await _email.SendAsync(
                                email,
                                "Ваша ставка перебита",
                                $"Вашу ставку на {mandarin.Name} перебили"
                            );

                            Console.WriteLine($"[EMAIL SENT] {email}");

                        }
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