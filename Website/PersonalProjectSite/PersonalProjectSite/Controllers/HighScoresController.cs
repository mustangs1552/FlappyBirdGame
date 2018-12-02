using Microsoft.AspNetCore.Mvc;
using PersonalProjectSite.Models;
using PersonalProjectSite.Models.DALs;

namespace PersonalProjectSite.Controllers
{
    public class HighScoresController : Controller
    {
        private const string connString = @"Data Source=.\SQLEXPRESS;Initial Catalog=PersonalGameSite;Integrated Security=true;";

        [HttpPost]
        public IActionResult SaveNewScore(string gameID, string username, string score)
        {
            HighScoresDAL dal = new HighScoresDAL(connString);
            HighScoresModel model = new HighScoresModel()
            {
                GameID = int.Parse(gameID),
                ScoreUsername = username,
                Score = int.Parse(score),
            };

            if (dal.AddHighScore(model) > 0) return StatusCode(200);
            else return StatusCode(300);
        }
    }
}