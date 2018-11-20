using Microsoft.AspNetCore.Mvc;
using PersonalProjectSite.Models;
using PersonalProjectSite.Models.DALs;

namespace PersonalProjectSite.Controllers
{
    public class PlayGameController : Controller
    {
        private const string connString = @"Data Source=.\SQLEXPRESS;Initial Catalog=PersonalGameSite;Integrated Security=true;";

        public IActionResult Index(int id)
        {
            GamesDAL dal = new GamesDAL(connString);
            return View(dal.GetGame(id));
        }
    }
}