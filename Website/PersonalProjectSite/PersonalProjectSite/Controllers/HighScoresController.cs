using Microsoft.AspNetCore.Mvc;
using PersonalProjectSite.Models;
using PersonalProjectSite.Models.DALs;

namespace PersonalProjectSite.Controllers
{
    public class HighScoresController : Controller
    {
        private const string connString = @"Data Source=.\SQLEXPRESS;Initial Catalog=PersonalGameSite;Integrated Security=true;";

        public IActionResult SaveNewScore(uint gameID, string username, int score)
        {
            HighScoresDAL dal = new HighScoresDAL(connString);
            HighScoresModel model = new HighScoresModel()
            {
                GameID = gameID,
                ScoreUsername = username,
                Score = score,
            };
            dal.AddHighScore(model);
            return View();
        }
    }
}