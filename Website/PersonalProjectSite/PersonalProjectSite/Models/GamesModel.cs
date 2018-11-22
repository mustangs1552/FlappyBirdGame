using System.Collections.Generic;

namespace PersonalProjectSite.Models
{
    public class GamesModel
    {
        public int GameID { get; set; }
        public string GameName { get; set; }
        public int GameType { get; set; }
        public string GameDescription { get; set; }
        public string GameSrc { get; set; }

        public List<HighScoresModel> HighScores { get; set; }
    }
}
