using Microsoft.AspNetCore.Mvc;

namespace PersonalProjectSite.Controllers
{
    public class PlayGameController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}