using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalProjectSite.Models
{
    public class GamesModel
    {
        public int GameID { get; set; }
        public string GameName { get; set; }
        public int GameType { get; set; }
        public string GameDescription { get; set; }
        public string GameSrc { get; set; }
    }
}
