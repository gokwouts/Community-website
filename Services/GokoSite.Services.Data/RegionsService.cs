namespace GokoSite.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Data;
    using GokoSite.Data.Models.LoL;
    using GokoSite.Web.ViewModels.Regions;

    public class RegionsService : IRegionsService
    {
        private readonly ApplicationDbContext db;

        public RegionsService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<ICollection<RegionsLolAppViewModel>> GetRegions()
        {
            var regions = this.db.Regions.ToList();

            if (regions.Count == 0)
            {
                await this.UpdateRegions();
                regions = this.db.Regions.OrderBy(r => r.RiotRegionId).ToList();
            }

            var convertedRegions = this.ConvertRegions(regions);

            return convertedRegions.ToList();
        }

        public async Task UpdateRegions()
        {
            foreach (var regionName in Enum.GetNames(typeof(RiotSharp.Misc.Region)))
            {
                this.db.Regions.Add(new Region()
                {
                    RegionName = regionName,
                    RiotRegionId = (int)Enum.Parse(typeof(RiotSharp.Misc.Region), regionName),
                });

                await this.db.SaveChangesAsync();
            }
        }

        private ICollection<RegionsLolAppViewModel> ConvertRegions(List<Region> regions)
        {
            var newRegions = new List<RegionsLolAppViewModel>();

            foreach (var region in regions)
            {
                Enum.TryParse(typeof(ConvertedRegions), region.RiotRegionId.ToString(), out var newRegionEnum);

                if ((int)newRegionEnum > Enum.GetNames(typeof(ConvertedRegions)).Count() - 1)
                {
                    continue;
                }

                var newRegion = new RegionsLolAppViewModel()
                {
                    Id = region.RiotRegionId,
                    Name = ((ConvertedRegions)newRegionEnum).ToString(),
                };

                newRegions.Add(newRegion);
            }

            return newRegions;
        }

        private enum ConvertedRegions
        {
            Brazil = 0,
            EUNE = 1,
            EUW = 2,
            NorthAmerica = 3,
            Korea = 4,
            LAN = 5,
            LAS = 6,
            Oceania = 7,
            Russia = 8,
            Turkey = 9,
            Japan = 10,
        }
    }
}
