using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace PersonalProjectSite.Models.DALs
{
    public class GamesDAL : IGamesDAL
    {
        private string connString = "";
        private const string SQL_GET_ALL = "SELECT * FROM Games;";
        private const string SQL_GET_GAME_NAME = "SELECT * FROM Games WHERE gameName = @gameName;";

        public GamesDAL(string connectionString)
        {
            connString = connectionString;
        }

        public List<GamesModel> GetAllGames()
        {
            return PerformSQL();
        }

        public GamesModel GetGame(string name)
        {
            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                { "@gameName", name },
            };
            return PerformSQL(SQL_GET_GAME_NAME, parameters)[0];
        }

        private List<GamesModel> PopulateGamesList(SqlDataReader reader)
        {
            List<GamesModel> output = new List<GamesModel>();
            while(reader.Read())
            {
                GamesModel model = new GamesModel();
                model.GameID = Convert.ToInt32(reader["gameID"]);
                model.GameName = Convert.ToString(reader["gameName"]);
                model.GameType = Convert.ToInt32(reader["gameType"]);
                model.GameDescription = Convert.ToString(reader["gameDescription"]);
                model.GameSrc = Convert.ToString(reader["gameSrc"]);
            }
            return output;
        }
        private List<GamesModel> PerformSQL(string sqlString = SQL_GET_ALL)
        {
            return PerformSQL(sqlString, null);
        }
        private List<GamesModel> PerformSQL(string sqlString, Dictionary<string, Object> parameters)
        {
            int temp = -1;
            return PerformSQL(sqlString, parameters, out temp);
        }
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

                    rowsAffected = cmd.ExecuteNonQuery();
                    return PopulateGamesList(cmd.ExecuteReader());
                }
            }
            catch (SqlException)
            {
                throw;
            }
        }
    }
}
