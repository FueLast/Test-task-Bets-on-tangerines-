using MandarinBid.Data;
using MandarinBid.Models;
using MandarinBid.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MandarinBid.Services.Implementations
{
    public class AuctionService : IAuctionService
    {
        private readonly ApplicationDbContext _db;

        public AuctionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Mandarin>> GetActiveMandarinsAsync()
        {
            return await _db.Mandarins
                .Include(m => m.Bids)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> PlaceBidAsync(int mandarinId, decimal amount, string userId)
        {
            var mandarin = await _db.Mandarins
                .FirstOrDefaultAsync(m => m.Id == mandarinId);

            if (mandarin == null)
                return false;

            // проверяем, что ставка выше текущей
            if (amount <= mandarin.CurrentPrice)
                return false;

            mandarin.CurrentPrice = amount;

            _db.Bids.Add(new Bid
            {
                MandarinId = mandarinId,
                Amount = amount,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await _db.SaveChangesAsync(); // тут может упасть из-за concurrency
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                // кто-то успел раньше
                return false;
            }
        }
    }
}