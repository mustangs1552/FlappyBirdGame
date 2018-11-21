using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;
using System.Collections.Generic;
using PersonalProjectSite.Models;
using PersonalProjectSite.Models.DALs;

namespace PersonalProjectSiteTests
{
    [TestClass]
    public class HighScoresDALTests
    {
        private const string connString = @"Data Source=.\SQLEXPRESS;Initial Catalog=PersonalGameSite_TEST;Integrated Security=true;";
        private TransactionScope trans = null;
        private IGamesDAL gamesDAL = null;
        private IHighScoresDAL hsDAL = null;

        [TestInitialize]
        public void Init()
        {
            gamesDAL = new GamesDAL(connString);
            hsDAL = new HighScoresDAL(connString);
            trans = new TransactionScope();

            GamesModel model = new GamesModel()
            {
                GameName = "TestNameOne",
                GameType = 0,
                GameDescription = "Test Description.",
                GameSrc = "www.test.com"
            };
            gamesDAL.AddGame(model);
            HighScoresModel hsModel = new HighScoresModel()
            {
                GameID = gamesDAL.GetGame("TestNameOne").GameID,
                ScoreUsername = "User",
                Score = 5
            };
            hsDAL.AddHighScore(hsModel);

            model = new GamesModel()
            {
                GameName = "TestNameTwo",
                GameType = 0,
                GameDescription = "Test Description.",
                GameSrc = "www.test.com"
            };
            gamesDAL.AddGame(model);
            hsModel = new HighScoresModel()
            {
                GameID = gamesDAL.GetGame("TestNameTwo").GameID,
                ScoreUsername = "User",
                Score = 5
            };
            hsDAL.AddHighScore(hsModel);
            hsModel = new HighScoresModel()
            {
                GameID = gamesDAL.GetGame("TestNameTwo").GameID,
                ScoreUsername = "User",
                Score = 5
            };
            hsDAL.AddHighScore(hsModel);

            model = new GamesModel()
            {
                GameName = "TestNameThree",
                GameType = 0,
                GameDescription = "Test Description.",
                GameSrc = "www.test.com"
            };
            gamesDAL.AddGame(model);
        }
        [TestCleanup]
        public void Cleanup()
        {
            trans.Dispose();
        }

        [TestMethod]
        public void AddHighScoresTest()
        {
            HighScoresModel model = new HighScoresModel()
            {
                GameID = gamesDAL.GetGame("TestNameThree").GameID,
                ScoreUsername = "User",
                Score = 5
            };

            Assert.AreEqual(1, hsDAL.AddHighScore(model));
        }
        [TestMethod]
        public void AddInvalidHighScoresTest()
        {
            Assert.AreEqual(0, hsDAL.AddHighScore(new HighScoresModel()));
        }

        [TestMethod]
        public void GetAllHighScoresTest()
        {
            List<HighScoresModel> scores = hsDAL.GetAllHighScores();

            Assert.IsNotNull(scores);
            Assert.AreEqual(3, scores.Count);
        }

        [TestMethod]
        public void GetHighScoresByNameTest()
        {
            List<HighScoresModel> scores = hsDAL.GetAllHighScores("TestNameTwo");

            Assert.IsNotNull(scores);
            Assert.AreEqual(gamesDAL.GetGame("TestNameTwo").GameID, scores[0].GameID);
        }
        [TestMethod]
        public void GetHighScoresByInvalidNameTest()
        {
            List<HighScoresModel> scores = hsDAL.GetAllHighScores(null);

            Assert.IsNotNull(scores);
        }
    }
}
