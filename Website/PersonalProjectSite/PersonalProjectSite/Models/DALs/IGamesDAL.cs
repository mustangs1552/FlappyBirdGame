using System.Collections.Generic;

namespace PersonalProjectSite.Models.DALs
{
    public interface IGamesDAL
    {
        List<GamesModel> GetAllGames();
        GamesModel GetGame(uint id);
        GamesModel GetGame(string name);
        int AddGame(GamesModel model);
    }
}
