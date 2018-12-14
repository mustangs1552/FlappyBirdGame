using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using PersonalProjectSite.Models;

namespace PersonalProjectSite.Models.DALs
{
    public class HighScoresDAL : IHighScoresDAL
    {
        private string connString = "";
        private const string SQL_GET_ALL = "SELECT * FROM HighScores ORDER BY HighScores.score DESC;";
        private const string SQL_GET_GAME_ID = "SELECT * FROM HighScores WHERE gameID = @gameID ORDER BY HighScores.score DESC;";
        private const string SQL_GET_GAME_ID_TOPX = "SELECT TOP (@topX) * FROM HighScores WHERE gameID = @gameID ORDER BY HighScores.score DESC;";
        private const string SQL_GET_GAME_NAME = "SELECT * FROM HighScores JOIN Games ON HighScores.gameID = Games.gameID WHERE Games.gameName = @gameName ORDER BY HighScores.score DESC;";
        private const string SQL_GET_GAME_NAME_TOPX = "SELECT TOP (@topX) * FROM HighScores JOIN Games ON HighScores.gameID = Games.gameID WHERE Games.gameName = @gameName ORDER BY HighScores.score DESC;";
        private const string SQL_ADD_HIGHSCORE = "INSERT INTO HighScores VALUES(@gameID, @scoreUsername, @score);";

        public HighScoresDAL(string connectionString)
        {
            connString = connectionString;
        }

        public List<HighScoresModel> GetAllHighScores()
        {
            return SQLUtilities.PerformSQL(connString, SQL_GET_ALL, PopulateList);
        }
        public List<HighScoresModel> GetAllHighScores(int id)
        {
            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                {"@gameID", id}
            };
            return SQLUtilities.PerformSQL(connString, SQL_GET_GAME_ID, parameters, PopulateList);
        }
        public List<HighScoresModel> GetAllHighScores(int id, int topX)
        {
            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                {"@topX", topX},
                {"@gameID", id}
            };
            return SQLUtilities.PerformSQL(connString, SQL_GET_GAME_ID_TOPX, parameters, PopulateList);
        }
        public List<HighScoresModel> GetAllHighScores(string name)
        {
            if(name == null)
            {
                return new List<HighScoresModel>();
            }

            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                {"@gameName", name}
            };
            return SQLUtilities.PerformSQL(connString, SQL_GET_GAME_NAME, parameters, PopulateList);
        }
        public List<HighScoresModel> GetAllHighScores(string name, int topX)
        {
            if (name == null)
            {
                return new List<HighScoresModel>();
            }

            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                {"@topX", topX},
                {"@gameName", name}
            };
            return SQLUtilities.PerformSQL(connString, SQL_GET_GAME_NAME_TOPX, parameters, PopulateList);
        }

        public int AddHighScore(HighScoresModel model)
        {
            if(model == null)
            {
                return 0;
            }
            if(model.ScoreUsername == null || model.ScoreUsername == "")
            {
                return 0;
            }

            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                {"@gameID", model.GameID},
                {"@scoreUsername", model.ScoreUsername},
                {"@score", model.Score},
            };
            int rowsAffected = -1;

            SQLUtilities.PerformSQL<HighScoresModel>(connString, SQL_ADD_HIGHSCORE, parameters, out rowsAffected);
            return rowsAffected;
        }
        
        /// <summary>
        /// Populates a list of models via the SqlDataReader provided.
        /// </summary>
        /// <param name="reader">The SqlDataReader to get the list from.</param>
        /// <returns>List of models.</returns>
        private List<HighScoresModel> PopulateList(SqlDataReader reader)
        {
            List<HighScoresModel> output = new List<HighScoresModel>();
            while (reader.Read())
            {
                HighScoresModel model = new HighScoresModel();
                model.GameID = Convert.ToInt32(reader["gameID"]);
                model.ScoreUsername = Convert.ToString(reader["scoreUsername"]);
                model.Score = Convert.ToInt32(reader["score"]);
                output.Add(model);
            }
            return output;
        }
    }
}
