using MandarinBid.Models;

namespace MandarinBid.Services.Interfaces
{
    public interface IAuctionService
    {
        Task<List<Mandarin>> GetActiveMandarinsAsync();

        Task<(bool Success, string Error)> PlaceBidAsync(int mandarinId, decimal amount, string userId);
    }

    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string body);
    }
    


}