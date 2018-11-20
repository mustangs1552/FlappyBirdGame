using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Transactions;
using System.Collections.Generic;
using PersonalProjectSite.Models;
using PersonalProjectSite.Models.DALs;

namespace PersonalProjectSiteTests
{
    [TestClass]
    public class GamesDALTests
    {
        private const string connString = @"Data Source=.\SQLEXPRESS;Initial Catalog=PersonalGameSite;Integrated Security=true;";
        private TransactionScope trans = null;
        private IGamesDAL dal = null;

        [TestInitialize]
        public void Init()
        {
            dal = new GamesDAL(connString);
            trans = new TransactionScope();

            GamesModel model = new GamesModel()
            {
                GameName = "TestNameOne",
                GameType = 0,
                GameDescription = "Test Description.",
                GameSrc = "www.test.com"
            };
            dal.AddGame(model);

            model = new GamesModel()
            {
                GameName = "TestNameTwo",
                GameType = 0,
                GameDescription = "Test Description.",
                GameSrc = "www.test.com"
            };
            dal.AddGame(model);

            model = new GamesModel()
            {
                GameName = "TestNameThree",
                GameType = 0,
                GameDescription = "Test Description.",
                GameSrc = "www.test.com"
            };
            dal.AddGame(model);
        }
        [TestCleanup]
        public void Cleanup()
        {
            trans.Dispose();
        }

        [TestMethod]
        public void AddGameTest()
        {
            GamesModel model = new GamesModel()
            {
                GameName = "TestName",
                GameType = 0,
                GameDescription = "Test Description.",
                GameSrc = "www.test.com"
            };

            Assert.AreEqual(1, dal.AddGame(model));
        }
        [TestMethod]
        public void AddInvalidGameTest()
        {
            Assert.AreEqual(0, dal.AddGame(new GamesModel()));
        }

        [TestMethod]
        public void GetAllGamesTest()
        {
            List<GamesModel> games = dal.GetAllGames();

            Assert.IsNotNull(games);
            Assert.AreEqual(3, games.Count);
        }

        [TestMethod]
        public void GetGameByNameTest()
        {
            GamesModel game = dal.GetGame("TestNameTwo");

            Assert.IsNotNull(game);
            Assert.AreEqual("TestNameTwo", game.GameName);
        }
        [TestMethod]
        public void GetGameByInvalidNameTest()
        {
            GamesModel game = dal.GetGame(null);

            Assert.IsNotNull(game);
        }
    }
}
