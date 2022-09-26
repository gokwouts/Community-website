namespace GokoSite.Services.Data.Tests
{
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Data;
    using GokoSite.Web.ViewModels.Regions;
    using Microsoft.EntityFrameworkCore;

    using Xunit;

    public class RegionsServiceTests
    {
        [Fact]
        public async Task GetRegionsShouldReturnCollectionOfRegionsLolAppViewModel()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTest");
            var db = new ApplicationDbContext(options.Options);

            var service = new RegionsService(db);

            var result = await service.GetRegions();

            Assert.NotNull(result);
            Assert.IsType<RegionsLolAppViewModel>(result.First());
        }
    }
}
