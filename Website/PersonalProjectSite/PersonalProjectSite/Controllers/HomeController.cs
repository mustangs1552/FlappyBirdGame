using Microsoft.AspNetCore.Mvc;
using PersonalProjectSite.Models.DALs;

namespace PersonalProjectSite.Controllers
{
    public class HomeController : Controller
    {
        private const string connString = @"Data Source=.\SQLEXPRESS;Initial Catalog=PersonalGameSite;Integrated Security=true;";

        public IActionResult Index()
        {
            GamesDAL dal = new GamesDAL(connString);
            return View(dal.GetAllGames());
        }
    }
}