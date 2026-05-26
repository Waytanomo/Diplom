using Microsoft.AspNetCore.Mvc;

namespace TicketMonitor.Api.Controllers.Pages;

public class DashboardPageController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}