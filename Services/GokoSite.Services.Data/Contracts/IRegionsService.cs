namespace GokoSite.Services.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using GokoSite.Web.ViewModels.Regions;

    public interface IRegionsService
    {
        public Task<ICollection<RegionsLolAppViewModel>> GetRegions();
    }
}
