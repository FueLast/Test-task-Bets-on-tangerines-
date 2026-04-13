using MandarinBid.Services.Interfaces;
using Microsoft.AspNetCore.Mvc; 
using System.Security.Claims;

namespace MandarinBid.Controllers
{
    public class AuctionController : Controller
    {
        private readonly IAuctionService _auctionService;

        private readonly ILogger<AuctionController> _logger;

        public AuctionController(
            IAuctionService auctionService,
            ILogger<AuctionController> logger)
        {
            _auctionService = auctionService;
            _logger = logger;
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

            if (!result.Success)
            {
                TempData["Error"] = result.Error;
                TempData["SuccessMandarinId"] = mandarinId;
            }
            else
            {
                TempData["Success"] = "Ставка принята";
                TempData["SuccessMandarinId"] = mandarinId;

                _logger.LogInformation("User {UserId} placing bid {Amount} on {MandarinId}",
                    userId, amount, mandarinId);
            }

            return RedirectToAction("Index");
             
        }


    }
}