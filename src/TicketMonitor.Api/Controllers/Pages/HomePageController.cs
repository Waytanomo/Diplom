using Microsoft.AspNetCore.Mvc;

namespace TicketMonitor.Api.Controllers.Pages;

public class HomePageController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
