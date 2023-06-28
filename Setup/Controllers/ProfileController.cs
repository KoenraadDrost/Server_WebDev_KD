using Microsoft.AspNetCore.Mvc;

namespace Setup.Controllers
{
    public class ProfileController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
