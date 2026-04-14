using MandarinBid.Models;

namespace MandarinBid.Services.Interfaces
{
    public interface IAuctionService
    {
        // получить список активных лотов
        Task<List<Mandarin>> GetActiveMandarinsAsync();

        // сделать ставку (возвращает success + текст ошибки)
        Task<(bool Success, string Error)> PlaceBidAsync(int mandarinId, decimal amount, string userId);
    }

    public interface IEmailService
    {
        // универсальный контракт отправки email
        Task SendAsync(string to, string subject, string body);
    }
}