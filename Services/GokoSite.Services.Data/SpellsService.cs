namespace GokoSite.Services.Data
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Services.Data.StaticData;
    using RiotSharp;

    public class SpellsService : ISpellsService
    {
        public RiotApi Api { get; set; }

        public SpellsService()
        {
            this.Api = RiotApi.GetDevelopmentInstance(PublicData.apiKey);
        }

        public async Task<string> GetSpellUrlById(int id)
        {
            var spells = await this.Api.StaticData.SummonerSpells.GetAllAsync(PublicData.ddVerision);
            var spell = spells.SummonerSpells.FirstOrDefault(s => s.Value.Id == id).Value;

            if (spell == null)
            {
                throw new InvalidOperationException("The id of the spell is invalid.");
            }

            string fullName = spell.Image.Full;

            return $"http://ddragon.leagueoflegends.com/cdn/{PublicData.ddVerision}/img/spell/{fullName}";
        }
    }
}
