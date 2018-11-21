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
            GamesModel model = dal.GetGame(id);
            model.HighScores = new HighScoresDAL(connString).GetAllHighScores(model.GameID, 10);
            return View(model);
        }
    }
}