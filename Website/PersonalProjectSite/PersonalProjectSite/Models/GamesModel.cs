using System.Collections.Generic;

namespace PersonalProjectSite.Models
{
    public class GamesModel
    {
        public uint GameID { get; set; }
        public string GameName { get; set; }
        public uint GameType { get; set; }
        public string GameDescription { get; set; }
        public string GameSrc { get; set; }

        public List<HighScoresModel> HighScores { get; set; }
    }
}
