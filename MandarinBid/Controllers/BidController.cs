using MandarinBid.Services.Implementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

public class BidController : Controller
{
    private readonly AuctionService _auctionService;

    public BidController(AuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    [HttpPost]
    [Authorize] // только авторизованные
    public async Task<IActionResult> Place(int mandarinId, decimal amount)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var success = await _auctionService.PlaceBidAsync(mandarinId, amount, userId);

        if (!success)
        {
            TempData["Error"] = "ставка должна быть выше текущей";
        }

        return RedirectToAction("Index", "Auction");
    }
}