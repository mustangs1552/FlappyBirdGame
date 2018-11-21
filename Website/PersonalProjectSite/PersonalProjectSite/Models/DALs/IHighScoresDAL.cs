using System.Collections.Generic;
using PersonalProjectSite.Models;

namespace PersonalProjectSite.Models.DALs
{
    public interface IHighScoresDAL
    {
        List<HighScoresModel> GetAllHighScores();
        List<HighScoresModel> GetAllHighScores(uint id);
        List<HighScoresModel> GetAllHighScores(uint id, uint topX);
        List<HighScoresModel> GetAllHighScores(string name);
        List<HighScoresModel> GetAllHighScores(string name, uint topX);
        int AddHighScore(HighScoresModel model);
    }
}
