using Microsoft.AspNetCore.Mvc;

namespace TicketMonitor.Api.Controllers.Pages;

public class AuthPageController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
}
