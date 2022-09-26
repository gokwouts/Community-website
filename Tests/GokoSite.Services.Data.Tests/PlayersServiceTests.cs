namespace GokoSite.Services.Data.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Services.Data.StaticData;
    using GokoSite.Web.ViewModels.Games.DTOs;
    using Moq;
    using RiotSharp;
    using RiotSharp.Endpoints.MatchEndpoint;
    using Xunit;

    public class PlayersServiceTests
    {
        private Mock<IChampionsService> championsService;
        private Mock<ISpellsService> spellsService;
        private RiotApi api;

        public PlayersServiceTests()
        {
            this.championsService = new Mock<IChampionsService>();
            this.spellsService = new Mock<ISpellsService>();
            this.api = RiotApi.GetDevelopmentInstance(PublicData.apiKey);
        }

        [Fact]
        public async Task GetPlayersByParticipantsShouldRetrunCollectionOfPlayers()
        {
            var gameId = 2652692459;
            var region = RiotSharp.Misc.Region.Eune;
            var game = await this.api.Match.GetMatchAsync(region, gameId);

            List<ParticipantIdentity> participantIdentities = game.ParticipantIdentities;
            List<Participant> participants = game.Participants;
            int teamId = 100;

            var firstPlayerExpectedUsername = "ELA RE KAKOPAIXT";
            var lastPlayerExpectedUsername = "KiwI 46";

            var service = new PlayersService(this.championsService.Object, this.spellsService.Object);

            var result = service.GetPlayersByParticipants(participantIdentities, participants, teamId);

            var firstPlayer = result.FirstOrDefault();
            var lastPlayer = result.LastOrDefault();

            Assert.NotNull(result);
            Assert.NotNull(firstPlayer);
            Assert.NotNull(lastPlayer);
            Assert.IsType<GokoSite.Data.Models.LoL.Player>(result.First());
            Assert.Equal(firstPlayerExpectedUsername, firstPlayer.Username);
            Assert.Equal(lastPlayerExpectedUsername, lastPlayer.Username);
        }

        [Fact]
        public async Task GetPlayersByParticipantsDtoShouldRetrunCollectionOfPlayerDTO()
        {
            var gameId = 4503146831;
            var region = RiotSharp.Misc.Region.Euw;
            var game = await this.api.Match.GetMatchAsync(region, gameId);

            List<ParticipantIdentity> participantIdentities = game.ParticipantIdentities;
            List<Participant> participants = game.Participants;
            int teamId = 100;

            var firstPlayerExpectedUsername = participantIdentities[0].Player.SummonerName;
            var firstPlayerExpectedCS = $"{participants[0].Stats.NeutralMinionsKilled + participants[0].Stats.TotalMinionsKilled}";
            var firstPlayerExpectedDamage = participants[0].Stats.TotalDamageDealtToChampions;
            var firstPlayerExpectedKDA = $"{participants[0].Stats.Kills}/{participants[0].Stats.Deaths}/{participants[0].Stats.Assists}";
            var firstPlayerExpectedLevel = (int)participants[0].Stats.ChampLevel;

            var lastPlayerExpectedUsername = participantIdentities[4].Player.SummonerName;
            var lastPlayerExpectedCS = $"{participants[4].Stats.NeutralMinionsKilled + participants[4].Stats.TotalMinionsKilled}";
            var lastPlayerExpectedDamage = participants[4].Stats.TotalDamageDealtToChampions;
            var lastPlayerExpectedKDA = $"{participants[4].Stats.Kills}/{participants[4].Stats.Deaths}/{participants[4].Stats.Assists}";
            var lastPlayerExpectedLevel = (int)participants[4].Stats.ChampLevel;

            var service = new PlayersService(this.championsService.Object, this.spellsService.Object);

            var result = await service.GetPlayersByParticipantsDto(participantIdentities, participants, teamId);

            var firstPlayer = result.FirstOrDefault();
            var lastPlayer = result.LastOrDefault();

            Assert.NotNull(result);
            Assert.NotNull(firstPlayer);
            Assert.NotNull(lastPlayer);
            Assert.IsType<PlayerDTO>(result.First());
            Assert.Equal(firstPlayerExpectedUsername, firstPlayer.Username);
            Assert.Equal(firstPlayerExpectedCS, firstPlayer.CS);
            Assert.Equal(firstPlayerExpectedDamage, firstPlayer.Damage);
            Assert.Equal(firstPlayerExpectedKDA, firstPlayer.KDA);
            Assert.Equal(firstPlayerExpectedLevel, firstPlayer.Level);
            Assert.Equal(lastPlayerExpectedUsername, lastPlayer.Username);
            Assert.Equal(lastPlayerExpectedCS, lastPlayer.CS);
            Assert.Equal(lastPlayerExpectedDamage, lastPlayer.Damage);
            Assert.Equal(lastPlayerExpectedKDA, lastPlayer.KDA);
            Assert.Equal(lastPlayerExpectedLevel, lastPlayer.Level);
        }
    }
}
