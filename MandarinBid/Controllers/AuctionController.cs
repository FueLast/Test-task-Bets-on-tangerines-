using MandarinBid.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MandarinBid.Controllers
{
    // контроллер, отвечающий за отображение аукциона
    public class AuctionController : Controller
    {
        // сервис с бизнес-логикой аукциона (получение лотов и т.д.)
        private readonly IAuctionService _auctionService;

        // внедрение зависимости через конструктор (dependency injection)
        public AuctionController(IAuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        // главная страница аукциона (список активных лотов)
        public async Task<IActionResult> Index()
        {
            // получаем только активные (не завершённые) лоты
            var mandarins = await _auctionService.GetActiveMandarinsAsync();

            // передаём данные во view
            return View(mandarins);
        }
    }
}