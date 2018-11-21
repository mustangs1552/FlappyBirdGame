using System.Collections.Generic;
using PersonalProjectSite.Models;

namespace PersonalProjectSite.Models.DALs
{
    public interface IHighScoresDAL
    {
        List<HighScoresModel> GetAllHighScores();
        List<HighScoresModel> GetAllHighScores(int id);
        List<HighScoresModel> GetAllHighScores(int id, int topX);
        List<HighScoresModel> GetAllHighScores(string name);
        List<HighScoresModel> GetAllHighScores(string name, int topX);
        int AddHighScore(HighScoresModel model);
    }
}
