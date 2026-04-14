using MandarinBid.Controllers;
using MandarinBid.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

// контроллер, отвечающий за действия со ставками
public class BidController : Controller
{
    // сервис с бизнес-логикой аукциона
    private readonly IAuctionService _auctionService;

    // логгер для отслеживания действий пользователей
    private readonly ILogger<AuctionController> _logger;

    // внедрение зависимостей
    public BidController(
        IAuctionService auctionService,
        ILogger<AuctionController> logger)
    {
        _auctionService = auctionService;
        _logger = logger;
    }

    // endpoint для размещения ставки
    // доступен только авторизованным пользователям
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Place(int mandarinId, decimal amount)
    {
        // получаем id текущего пользователя из claims
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // базовая серверная валидация (защита от некорректного ввода)
        if (amount <= 0)
        {
            TempData["Error"] = "Введите сумму ставки";
            TempData["SuccessMandarinId"] = mandarinId;

            // редирект обратно на страницу аукциона
            return RedirectToAction("Index", "Auction");
        }

        // вызываем бизнес-логику размещения ставки
        var result = await _auctionService.PlaceBidAsync(mandarinId, amount, userId);

        // если ставка не прошла (например, меньше текущей)
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            TempData["SuccessMandarinId"] = mandarinId;
        }
        else
        {
            // успешная ставка
            TempData["Success"] = "Ставка принята";
            TempData["SuccessMandarinId"] = mandarinId;

            // логируем факт ставки (для аналитики и дебага)
            _logger.LogInformation(
                "bid placed: user {UserId}, amount {Amount}, mandarin {MandarinId}",
                userId, amount, mandarinId);
        }

        // всегда возвращаем пользователя на список аукционов
        return RedirectToAction("Index", "Auction");
    }
}