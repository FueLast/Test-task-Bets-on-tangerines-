using MandarinBid.Services.Interfaces;
using Microsoft.AspNetCore.Mvc; 
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
    }
}