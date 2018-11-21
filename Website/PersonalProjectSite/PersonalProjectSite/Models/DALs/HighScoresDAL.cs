using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using PersonalProjectSite.Models;

namespace PersonalProjectSite.Models.DALs
{
    public class HighScoresDAL : IHighScoresDAL
    {
        private string connString = "";
        private const string SQL_GET_ALL = "SELECT * FROM HighScores;";
        private const string SQL_GET_GAME_ID = "SELECT * FROM HighScores WHERE gameID = @gameID;";
        private const string SQL_GET_GAME_ID_TOPX = "SELECT TOP (@topX) * FROM HighScores WHERE gameID = @gameID;";
        private const string SQL_GET_GAME_NAME = "SELECT * FROM HighScores JOIN Games ON HighScores.gameID = Games.gameID WHERE Games.gameName = @gameName;";
        private const string SQL_GET_GAME_NAME_TOPX = "SELECT TOP (@topX) * FROM HighScores JOIN Games ON HighScores.gameID = Games.gameID WHERE Games.gameName = @gameName;";
        private const string SQL_ADD_HIGHSCORE = "INSERT INTO HighScores VALUES(@gameID, @scoreUsername, @score);";

        public HighScoresDAL(string connectionString)
        {
            connString = connectionString;
        }

        public List<HighScoresModel> GetAllHighScores()
        {
            return PerformSQL();
        }
        public List<HighScoresModel> GetAllHighScores(uint id)
        {
            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                {"@gameID", id}
            };
            return PerformSQL(SQL_GET_GAME_ID, parameters);
        }
        public List<HighScoresModel> GetAllHighScores(uint id, uint topX)
        {
            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                {"@topX", topX},
                {"@gameID", id}
            };
            return PerformSQL(SQL_GET_GAME_ID_TOPX, parameters);
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
            return PerformSQL(SQL_GET_GAME_NAME, parameters);
        }
        public List<HighScoresModel> GetAllHighScores(string name, uint topX)
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
            return PerformSQL(SQL_GET_GAME_NAME_TOPX, parameters);
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

            PerformSQL(SQL_ADD_HIGHSCORE, parameters, out rowsAffected);
            return rowsAffected;
        }

        #region SQL
        /// <summary>
        /// Populates a list of models via the SqlDataReader provided.
        /// </summary>
        /// <param name="reader">The SqlDataReader to get the list from.</param>
        /// <returns>List of models.</returns>
        private List<HighScoresModel> PopulateGamesList(SqlDataReader reader)
        {
            List<HighScoresModel> output = new List<HighScoresModel>();
            while (reader.Read())
            {
                HighScoresModel model = new HighScoresModel();
                model.GameID = Convert.ToUInt32(reader["gameID"]);
                model.ScoreUsername = Convert.ToString(reader["scoreUsername"]);
                model.Score = Convert.ToInt32(reader["score"]);
                output.Add(model);
            }
            return output;
        }

        /// <summary>
        /// Executes a non-parameterized query and returns result.
        /// </summary>
        /// <param name="sqlString">The SQL string to execute.</param>
        /// <returns>The list of models found.</returns>
        private List<HighScoresModel> PerformSQL(string sqlString = SQL_GET_ALL)
        {
            return PerformSQL(sqlString, null);
        }
        /// <summary>
        /// Executes a parameterized query and returns result.
        /// </summary>
        /// <param name="sqlString">The SQL string to execute.</param>
        /// <param name="parameters">Dictionary of the parameters in the format: Key=@param, Value=value.</param>
        /// <returns>The list of models found.</returns>
        private List<HighScoresModel> PerformSQL(string sqlString, Dictionary<string, Object> parameters)
        {
            int temp = -1;
            return PerformSQL(sqlString, parameters, out temp);
        }
        /// <summary>
        /// Executes a parameterized query and returns result if the sqlString begins with "SELECT" otherwise will update rowsAddected with SQL rows affected.
        /// </summary>
        /// <param name="sqlString">The SQL string to execute.</param>
        /// <param name="parameters">Dictionary of the parameters in the format: Key=@param, Value=value.</param>
        /// <param name="rowsAffected">SQL rows affected.</param>
        /// <returns>The list of models found.</returns>
        private List<HighScoresModel> PerformSQL(string sqlString, Dictionary<string, Object> parameters, out int rowsAffected)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    if (parameters != null)
                    {
                        foreach (KeyValuePair<string, Object> pair in parameters)
                        {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                        }
                    }

                    if (sqlString.Substring(0, 6).ToUpper() != "SELECT")
                    {
                        rowsAffected = cmd.ExecuteNonQuery();
                        return new List<HighScoresModel>();
                    }
                    else
                    {
                        rowsAffected = -1;
                        return PopulateGamesList(cmd.ExecuteReader());
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
        }
        #endregion
    }
}
