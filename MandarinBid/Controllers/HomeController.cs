using MandarinBid.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MandarinBid.Controllers
{
    // базовый контроллер для статических страниц
    public class HomeController : Controller
    {
        // логгер (можно использовать для отслеживания ошибок/событий)
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // главная страница (landing)
        public IActionResult Index()
        {
            return View();
        }

        // страница политики конфиденциальности
        public IActionResult Privacy()
        {
            return View();
        }

        // обработка ошибок приложения
        // отключаем кэширование, чтобы всегда видеть актуальную ошибку
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // передаём request id для трассировки ошибки
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}