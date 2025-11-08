using Microsoft.AspNetCore.Mvc;

namespace Calendar.Controllers;

public class CalendarController : Controller
{

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}