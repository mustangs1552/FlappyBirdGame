﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Utilities;

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
            return SQLUtilities.PerformSQL(connString, SQL_GET_ALL, PopulateList);
        }

        public GamesModel GetGame(int id)
        {
            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                { "@gameID", id },
            };
            return SQLUtilities.PerformSQL(connString, SQL_GET_GAME_ID, parameters, PopulateList)[0];
        }
        /// <summary>
        /// Get the game with the given name.
        /// </summary>
        /// <param name="name">The name of the game to get.</param>
        /// <returns>The game found.</returns>
        public GamesModel GetGame(string name)
        {
            if (name == null)
            {
                return new GamesModel();
            }

            Dictionary<string, Object> parameters = new Dictionary<string, object>()
            {
                { "@gameName", name },
            };
            return SQLUtilities.PerformSQL(connString, SQL_GET_GAME_NAME, parameters, PopulateList)[0];
        }

        /// <summary>
        /// Adds the given game model to the database.
        /// </summary>
        /// <param name="model">The game to add.</param>
        /// <returns>The number of SQL rows affected.</returns>
        public int AddGame(GamesModel model)
        {
            if (model == null)
            {
                return 0;
            }
            if (model.GameName == null || model.GameName == "")
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

            SQLUtilities.PerformSQL<GamesModel>(connString, SQL_ADD_GAME, parameters, out rowsAffected);
            return rowsAffected;
        }

        /// <summary>
        /// Populates a list of models via the SqlDataReader provided.
        /// </summary>
        /// <param name="reader">The SqlDataReader to get the list from.</param>
        /// <returns>List of models.</returns>
        private GamesModel PopulateList(SqlDataReader reader)
        {
            GamesModel model = new GamesModel();
            model.GameID = Convert.ToInt32(reader["gameID"]);
            model.GameName = Convert.ToString(reader["gameName"]);
            model.GameType = Convert.ToInt32(reader["gameTypeID"]);
            model.GameDescription = Convert.ToString(reader["gameDiscription"]);
            model.GameSrc = Convert.ToString(reader["gameSrc"]);
            return model;
        }
    }
}
