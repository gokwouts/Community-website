namespace GokoSite.Services.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Data;
    using GokoSite.Data.Models;
    using GokoSite.Data.Models.RP;
    using GokoSite.Web.ViewModels.Forums;
    using Microsoft.EntityFrameworkCore;

    using Xunit;

    public class SpellsServiceTests
    {
        [Fact]
        public async Task GetSpellUrlByIdShouldReturnTheImageUrlOfTheSpell()
        {
            int spellId = 4;
            string expectedResultUrl = "http://ddragon.leagueoflegends.com/cdn/10.25.1/img/spell/SummonerFlash.png";

            var service = new SpellsService();

            var result = await service.GetSpellUrlById(spellId);

            Assert.Equal(expectedResultUrl, result);
        }

        [Fact]
        public async Task GetSpellUrlByIdShouldThrowInvalidOperationExceptionIfGivenInvalidId()
        {
            int invalidSpellId = 1322;
            var service = new SpellsService();

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.GetSpellUrlById(invalidSpellId));
        }
    }
}
