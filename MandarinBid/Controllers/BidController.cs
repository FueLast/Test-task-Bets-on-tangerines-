using MandarinBid.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class BidController : Controller
{
    private readonly IAuctionService _auctionService;

    public BidController(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Place(int mandarinId, decimal amount)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // защита от пустого ввода (серверная)
        if (amount <= 0)
        {
            TempData["Error"] = "Введите сумму ставки";
            TempData["SuccessMandarinId"] = mandarinId;
            return RedirectToAction("Index", "Auction");
        }

        var result = await _auctionService.PlaceBidAsync(mandarinId, amount, userId);

        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            TempData["SuccessMandarinId"] = mandarinId;
        }
        else
        {
            TempData["Success"] = "Ставка принята";
            TempData["SuccessMandarinId"] = mandarinId;
        }

        return RedirectToAction("Index", "Auction");
    }
}