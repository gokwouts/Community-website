namespace GokoSite.Services.Data.Tests
{
    using System;
    using System.Threading.Tasks;

    using GokoSite.Data;
    using GokoSite.Web.ViewModels.Games.DTOs;
    using Microsoft.EntityFrameworkCore;

    using Xunit;

    public class ChampionsServiceTests
    {
        [Fact]
        public async Task GetChampionShouldReturnTheChampionWithTheGivenId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("championTest");
            var db = new ApplicationDbContext(options.Options);

            int expectedChampionId = 33;
            string expectedChampionName = "Gangplank";

            var service = new ChampionsService(db);

            var result = await service.GetChampion(expectedChampionId);

            Assert.NotNull(result);
            Assert.Equal(expectedChampionId, result.ChampionId);
            Assert.Equal(expectedChampionName, result.ChampionName);
        }

        [Fact]
        public async Task GetChampionShouldThrowArgumentNullExceptionIfGivenInvalidId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("championTest");
            var db = new ApplicationDbContext(options.Options);

            int invalidChampionId = -100;

            var service = new ChampionsService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GetChampion(invalidChampionId));
        }

        [Fact]
        public async Task GetChampionDtoShouldReturnChampionDtoWithTheGivenId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase("championDtoTest");
            var db = new ApplicationDbContext(options.Options);

            int championRiotId = 41;
            string expectedChampionName = "Gangplank";

            var service = new ChampionsService(db);

            var result = await service.GetChampionDto(championRiotId);

            Assert.NotNull(result);
            Assert.Equal(expectedChampionName, result.ChampionName);
            Assert.IsType<ChampionDTO>(result);
        }

        [Fact]
        public async Task GetChampionDtoShouldThrowArgumentNullExceptionIfGivenInvalidRiotId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("championTest");
            var db = new ApplicationDbContext(options.Options);

            int invalidChampionRiotId = -100;

            var service = new ChampionsService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GetChampionDto(invalidChampionRiotId));
        }
    }
}
