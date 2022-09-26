namespace GokoSite.Services.Data
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Data;
    using GokoSite.Data.Models.LoL;
    using GokoSite.Services.Data.StaticData;
    using GokoSite.Web.ViewModels.Games.DTOs;
    using RiotSharp;

    public class ChampionsService : IChampionsService
    {
        private string ddVersion { get; set; }

        public RiotApi api { get; set; }

        private readonly ApplicationDbContext db;

        public ChampionsService(ApplicationDbContext db)
        {
            this.db = db;
            this.api = RiotApi.GetDevelopmentInstance(PublicData.apiKey);
            this.ddVersion = PublicData.ddVerision;
        }

        public async Task<Champion> GetChampion(int championId)
        {
            if (this.db.ChampionsStatic.ToList().Count == 0)
            {
                await this.UploadChamionsToDBAsync();
            }

            var champion = this.db.ChampionsStatic.FirstOrDefault(c => c.ChampionId == championId);

            if (champion == null)
            {
                throw new ArgumentNullException("championId", "The champion Id is invalid.");
            }

            return champion;
        }

        public async Task<ChampionDTO> GetChampionDto(int championRiotId)
        {
            if (this.db.ChampionsStatic.ToList().Count == 0)
            {
                await this.UploadChamionsToDBAsync();
            }

            var champion = this.db.ChampionsStatic
                .Where(c => c.ChampionRiotId == championRiotId.ToString())
                .Select(c => new ChampionDTO
                {
                    ChampionIconUrl = c.ChampionIconUrl,
                    ChampionName = c.ChampionName,
                })
                .FirstOrDefault();

            if (champion == null)
            {
                throw new ArgumentNullException("championRiotId", "The champion riot Id is invalid.");
            }

            return champion;
        }

        public async Task UploadChamionsToDBAsync()
        {
            var dic = await this.api.StaticData.Champions.GetAllAsync(this.ddVersion);
            var champions = dic.Champions.Values;

            foreach (var champ in champions)
            {
                var champion = new Champion
                {
                    ChampionName = champ.Name,
                    ChampionIconUrl = $"http://ddragon.leagueoflegends.com/cdn/{this.ddVersion}/img/champion/{champ.Image.Full}",
                    ChampionRiotId = champ.Id.ToString(),
                };

                this.db.ChampionsStatic.Add(champion);
            }

            await this.db.SaveChangesAsync();
        }
    }
}
