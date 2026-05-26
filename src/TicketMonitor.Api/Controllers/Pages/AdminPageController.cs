using Microsoft.AspNetCore.Mvc;

namespace TicketMonitor.Api.Controllers.Pages;

public class AdminPageController : Controller
{
    public IActionResult CreateUser()
    {
        return View();
    }
}