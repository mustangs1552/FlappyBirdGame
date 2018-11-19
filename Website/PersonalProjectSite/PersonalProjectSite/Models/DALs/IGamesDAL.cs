using System.Collections.Generic;

namespace PersonalProjectSite.Models.DALs
{
    public interface IGamesDAL
    {
        List<GamesModel> GetAllGames();
        GamesModel GetGame(string name);
    }
}
