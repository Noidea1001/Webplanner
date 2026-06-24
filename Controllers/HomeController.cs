using Microsoft.AspNetCore.Mvc;

namespace WebPlanner.Controllers;

public class HomeController : Controller
{
    [HttpGet] 
    [Route("/Home/Error")]
    public IActionResult Error()
    {
        return View();
    }
}
