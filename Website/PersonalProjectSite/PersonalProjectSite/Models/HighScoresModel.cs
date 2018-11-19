using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalProjectSite.Models
{
    public class HighScoresModel
    {
        public int ScoreID { get; set; }
        public int GameID { get; set; }
        public string ScoreUsername { get; set; }
        public int Score { get; set; }
    }
}
