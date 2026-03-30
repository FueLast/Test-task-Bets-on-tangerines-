using MandarinBid.Models;

namespace MandarinBid.Services.Interfaces
{
    public interface IAuctionService
    {
        Task<List<Mandarin>> GetActiveMandarinsAsync();

        Task<bool> PlaceBidAsync(int mandarinId, decimal amount, string userId);
    }
}