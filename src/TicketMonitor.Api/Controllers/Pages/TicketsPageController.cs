using Microsoft.AspNetCore.Mvc;

namespace TicketMonitor.Api.Controllers.Pages;

public class TicketsPageController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Create()
    {
        return View();
    }

    public IActionResult Details(int id)
    {
        return View();
    }
}