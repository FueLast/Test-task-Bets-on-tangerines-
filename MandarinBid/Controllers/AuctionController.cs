using MandarinBid.Models;
using MandarinBid.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MandarinBid.Controllers
{
    public class AuctionController : Controller
    {
        private readonly IAuctionService _auctionService;

        public AuctionController(IAuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        public async Task<IActionResult> Index()
        {
            var mandarins = await _auctionService.GetActiveMandarinsAsync();

            return View(mandarins);
        } 

        [HttpPost]
        public async Task<IActionResult> PlaceBid(int mandarinId, decimal amount)
        {
            if (!User.Identity.IsAuthenticated)
                return Unauthorized();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await _auctionService.PlaceBidAsync(mandarinId, amount, userId);

            if (!result)
            {
                TempData["Error"] = "Ставка должна быть выше текущей";
            }
            else
            {
                TempData["Success"] = "Ставка принята";
                TempData["SuccessMandarinId"] = mandarinId;
            }

            return RedirectToAction("Index");
        }


    }
}