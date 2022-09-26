namespace GokoSite.Services.Data.Tests
{
    using System.Collections.Generic;

    using GokoSite.Web.ViewModels.Players;

    using Xunit;

    public class RPServerServiceTests
    {
        [Fact]
        public void GetPlayersShouldReturnHomePageViewModelWithPlayersAndTheirPing()
        {
            var service = new RPServerService();

            var result = service.GetPlayers();

            Assert.NotNull(result);
            Assert.NotNull(result.PlayerNames);
            Assert.NotNull(result.PlayerPings);
            Assert.True(result.PlayersCount >= 0);
            Assert.IsType<HomePageViewModel>(result);
            Assert.IsType<List<int>>(result.PlayerPings);
            Assert.IsType<List<string>>(result.PlayerNames);
            Assert.IsType<HomePageViewModel>(result);
        }
    }
}
