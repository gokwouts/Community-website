namespace GokoSite.Services.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using GokoSite.Web.ViewModels.Games;
    using RiotSharp.Endpoints.MatchEndpoint;
    using RiotSharp.Endpoints.SummonerEndpoint;

    public interface IGamesService
    {
        Task<ICollection<Match>> GetGamesAsync(GetGamesInputModel input);

        Task<ICollection<CollectionPageGameViewModel>> GetCollectionGames(string userId);

        Task<Match> GetGameAsync(long gameId, RiotSharp.Misc.Region region);

        Task<Summoner> GetBasicSummonerDataAsync(string summonerName, RiotSharp.Misc.Region region);

        Task<IEnumerable<HomePageGameViewModel>> GetModelByMatches(ICollection<Match> games, int regionId);

        Task<HomePageGameViewModel> GetModelByGameId(long gameId, int regionId, string userId);

        Task AddGameToCollection(long gameId, int regionId);

        Task AddGameToUser(string userId, long riotGameId);

        Task RemoveGameFromCollection(string userId, long gameId);

        int GetGameCount(string userId);
    }
}
