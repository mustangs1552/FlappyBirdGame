using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace PersonalProjectSite.Models.DALs
{
    public class GamesDAL : IGamesDAL
    {
        private string connString = "";
        private const string SQL_GET_ALL = "SELECT * FROM Games;";
        private const string SQL_GET_GAME_ID = "SELECT * FROM Games WHERE gameID = @gameID;";
        private const string SQL_GET_GAME_NAME = "SELECT * FROM Games WHERE gameName = @gameName;";
        private const string SQL_ADD_GAME = "INSERT INTO Games VALUES(@gameName, @gameTypeID, @gameDescription, @gameSrc);";

        public GamesDAL(string connectionString)
        {
            connString = connectionString;
        }
        
        /// <summary>
        /// Gets all the games in the database.
        /// </summary>
        /// <returns>All the games in the database.</returns>
        public List<GamesModel> GetAllGames()
        {
            return PerformSQL();
        }

        public GamesModel GetGame(int id)
        {
            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                { "@gameID", id },
            };
            return PerformSQL(SQL_GET_GAME_ID, parameters)[0];
        }
        /// <summary>
        /// Get the game with the given name.
        /// </summary>
        /// <param name="name">The name of the game to get.</param>
        /// <returns>The game found.</returns>
        public GamesModel GetGame(string name)
        {
            if(name == null)
            {
                return new GamesModel();
            }

            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                { "@gameName", name },
            };
            return PerformSQL(SQL_GET_GAME_NAME, parameters)[0];
        }

        /// <summary>
        /// Adds the given game model to the database.
        /// </summary>
        /// <param name="model">The game to add.</param>
        /// <returns>The number of SQL rows affected.</returns>
        public int AddGame(GamesModel model)
        {
            if(model == null)
            {
                return 0;
            }
            if(model.GameName == null || model.GameName == "")
            {
                return 0;
            }
            if (model.GameDescription == null)
            {
                model.GameDescription = "";
            }
            if (model.GameSrc == null || model.GameSrc == "")
            {
                return 0;
            }

            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                { "@gameName", model.GameName },
                {"@gameTypeID", model.GameType },
                {"@gameDescription", model.GameDescription },
                {"@gameSrc", model.GameSrc }
            };
            int rowsAffected = -1;

            PerformSQL(SQL_ADD_GAME, parameters, out rowsAffected);
            return rowsAffected;
        }

        #region SQL
        /// <summary>
        /// Populates a list of models via the SqlDataReader provided.
        /// </summary>
        /// <param name="reader">The SqlDataReader to get the list from.</param>
        /// <returns>List of models.</returns>
        private List<GamesModel> PopulateGamesList(SqlDataReader reader)
        {
            List<GamesModel> output = new List<GamesModel>();
            while(reader.Read())
            {
                GamesModel model = new GamesModel();
                model.GameID = Convert.ToInt32(reader["gameID"]);
                model.GameName = Convert.ToString(reader["gameName"]);
                model.GameType = Convert.ToInt32(reader["gameTypeID"]);
                model.GameDescription = Convert.ToString(reader["gameDiscription"]);
                model.GameSrc = Convert.ToString(reader["gameSrc"]);
                output.Add(model);
            }
            return output;
        }

        /// <summary>
        /// Executes a non-parameterized query and returns result.
        /// </summary>
        /// <param name="sqlString">The SQL string to execute.</param>
        /// <returns>The list of models found.</returns>
        private List<GamesModel> PerformSQL(string sqlString = SQL_GET_ALL)
        {
            return PerformSQL(sqlString, null);
        }
        /// <summary>
        /// Executes a parameterized query and returns result.
        /// </summary>
        /// <param name="sqlString">The SQL string to execute.</param>
        /// <param name="parameters">Dictionary of the parameters in the format: Key=@param, Value=value.</param>
        /// <returns>The list of models found.</returns>
        private List<GamesModel> PerformSQL(string sqlString, Dictionary<string, Object> parameters)
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
        private List<GamesModel> PerformSQL(string sqlString, Dictionary<string, Object> parameters, out int rowsAffected)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sqlString, conn);
                    if(parameters != null)
                    {
                        foreach(KeyValuePair<string, Object> pair in parameters)
                        {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                        }
                    }

                    if (sqlString.Substring(0, 6).ToUpper() != "SELECT")
                    {
                        rowsAffected = cmd.ExecuteNonQuery();
                        return new List<GamesModel>();
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
