namespace GokoSite.Services.Data
{
    using System.Threading.Tasks;

    using RiotSharp.Endpoints.StaticDataEndpoint.SummonerSpell;

    public interface ISpellsService
    {
        public Task<string> GetSpellUrlById(int id);
    }
}
