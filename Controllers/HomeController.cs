using Microsoft.AspNetCore.Mvc;

namespace WebPlanner.Controllers;

public class HomeController : Controller
{
    [AcceptVerbs("GET", "HEAD")] 
    [Route("/Home/Error")]
    public IActionResult Error()
    {
        return View();
    }
}
