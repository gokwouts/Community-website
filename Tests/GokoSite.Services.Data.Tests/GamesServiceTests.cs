
namespace GokoSite.Services.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Data;
    using GokoSite.Data.Models;
    using GokoSite.Data.Models.LoL;
    using GokoSite.Services.Data.StaticData;
    using GokoSite.Web.ViewModels.Games;
    using GokoSite.Web.ViewModels.Games.DTOs;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using RiotSharp;

    using Xunit;

    using Region = RiotSharp.Misc.Region;

    public class GamesServiceTests
    {
        private Mock<ITeamsService> teamsService;
        private IPlayersService playersService;
        private RiotApi api;
        private Mock<IChampionsService> championsService;
        private Mock<ISpellsService> spellsService;

        public GamesServiceTests()
        {
            this.championsService = new Mock<IChampionsService>();
            this.spellsService = new Mock<ISpellsService>();
            this.playersService = new PlayersService(this.championsService.Object, this.spellsService.Object);
            this.teamsService = new Mock<ITeamsService>();
            this.api = RiotApi.GetDevelopmentInstance(PublicData.apiKey);
        }

        [Fact]
        public async Task GetLatestVersionShouldReturnDDragonNewestVersion()
        {
            string currentLatestVersion = "10.25.1";

            var result = PublicData.ddVerision;

            Assert.NotNull(result);
            Assert.Equal(currentLatestVersion, result);
        }

        [Fact]
        public async Task GetBasicSummonerDataAsyncShouldReturnSummonerBySummonerName()
        {
            string username = "Nikolcho";
            var region = Region.Eune;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGet");
            var db = new ApplicationDbContext(options.Options);

            int expectedProfileIconId = 4813;
            string expectedAccountId = "-0TmH2cZiK9xbRvB8GjSArr79ZNl2eWKLwuyFSHITqr8ow";

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            var result = await service.GetBasicSummonerDataAsync(username, region);

            Assert.NotNull(result);
            Assert.Equal(username, result.Name);
            Assert.Equal(region, result.Region);
            Assert.Equal(expectedProfileIconId, result.ProfileIconId);
            Assert.Equal(expectedAccountId, result.AccountId);
        }

        [Fact]
        public async Task GetBasicSummonerDataAsyncShouldReturnNullIfGivenInvalidSummonerName()
        {
            string username = "afweawefgrawrgawfwrafrw";
            var region = Region.Eune;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGetWrong");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            var result = await service.GetBasicSummonerDataAsync(username, region);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetGameAsyncShouldReturnMatchWithDataForTheGame()
        {
            long gameId = 2652692459;
            var region = Region.Eune;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGet");
            var db = new ApplicationDbContext(options.Options);

            int expectedSeasonId = 13;
            string expectedGameMode = "CLASSIC";
            int expectedQueueId = 420;
            string seventhParticipantExpectedUsername = "lnvictum";
            int firstTeamThirdBanChampionId = 58;

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            var result = await service.GetGameAsync(gameId, region);

            Assert.NotNull(result);
            Assert.IsType<RiotSharp.Endpoints.MatchEndpoint.Match>(result);
            Assert.Equal(gameId, result.GameId);
            Assert.Equal(expectedQueueId, result.QueueId);
            Assert.Equal(expectedGameMode, result.GameMode);
            Assert.Equal(expectedSeasonId, result.SeasonId);
            Assert.Equal(firstTeamThirdBanChampionId, result.Teams[0].Bans[2].ChampionId);
            Assert.Equal(seventhParticipantExpectedUsername, result.ParticipantIdentities[6].Player.SummonerName);
        }

        [Fact]
        public async Task GetGameAsyncShouldReturnNullIfGivenInvalidGameId()
        {
            long gameId = -321312312;
            var region = Region.Eune;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGet");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            var result = await service.GetGameAsync(gameId, region);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetGamesAsyncShouldReturnCollectionoFMatchByInput()
        {
            var summonerName = "Nikolcho";
            var count = 2;
            var region = Region.Eune;

            var summoner = await this.api.Summoner.GetSummonerByNameAsync(region, summonerName);

            var matchList = await this.api.Match.GetMatchListAsync(region, summoner.AccountId);
            var expectedGames = new List<RiotSharp.Endpoints.MatchEndpoint.Match>();

            for (int i = 0; i < count; i++)
            {
                var game = await this.api.Match.GetMatchAsync(region, matchList.Matches[i].GameId);

                expectedGames.Add(game);
            }

            GetGamesInputModel input = new GetGamesInputModel()
            {
                Username = summonerName,
                Count = count,
                RegionId = 1, // Eune
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGetMatches");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            var games = (await service.GetGamesAsync(input)).ToList();

            Assert.NotNull(games);
            Assert.IsType<List<RiotSharp.Endpoints.MatchEndpoint.Match>>(games);
            Assert.Equal(expectedGames[0].GameId, games[0].GameId);
            Assert.Equal(expectedGames[0].GameCreation, games[0].GameCreation);
            Assert.Equal(expectedGames[0].QueueId, games[0].QueueId);
            Assert.Equal(expectedGames[0].SeasonId, games[0].SeasonId);
            Assert.Equal(expectedGames[1].GameId, games[1].GameId);
            Assert.Equal(expectedGames[1].GameCreation, games[1].GameCreation);
            Assert.Equal(expectedGames[1].QueueId, games[1].QueueId);
            Assert.Equal(expectedGames[1].SeasonId, games[1].SeasonId);
        }

        [Theory]
        [InlineData("wafewarwa")]
        [InlineData("1321312321")]
        [InlineData("mamffaafa")]
        [InlineData("xasdgwafwaf")]
        public async Task GetGamesAsyncShouldThrowArgumentNullExceptionIfGivenNonExistentUsername(string username)
        {
            GetGamesInputModel input = new GetGamesInputModel()
            {
                Username = username,
                Count = 1,
                RegionId = 1, // Eune
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGetMatches");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GetGamesAsync(input));
        }

        [Theory]
        [InlineData("")]
        [InlineData("qwqwqwqwqwqwqwqwqwqw")]
        [InlineData("fewefweagrwokfapworwokwarof")]
        [InlineData("1234567891011121314")]
        public async Task GetGamesAsyncShouldThrowArgumentExceptionIfGivenInvalidUsername(string username)
        {
            GetGamesInputModel input = new GetGamesInputModel()
            {
                Username = username,
                Count = 1,
                RegionId = 1, // Eune
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGetMatches");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.GetGamesAsync(input));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(12)]
        [InlineData(18)]
        [InlineData(-12313)]
        public async Task GetGamesAsyncShouldThrowArgumentExceptionIfGivenInvalidCount(int count)
        {
            GetGamesInputModel input = new GetGamesInputModel()
            {
                Username = "Nikolcho",
                Count = count,
                RegionId = 1, // Eune
            };

            GetGamesInputModel secondInput = new GetGamesInputModel()
            {
                Username = "Nikolcho",
                Count = count,
                RegionId = 1, // Eune
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGetMatches");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.GetGamesAsync(input));
            await Assert.ThrowsAsync<ArgumentException>(async () => await service.GetGamesAsync(secondInput));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(12)]
        [InlineData(18)]
        [InlineData(-12313)]
        public async Task GetGamesAsyncShouldThrowArgumentExceptionIfGivenInvalidRegionId(int regionId)
        {
            GetGamesInputModel input = new GetGamesInputModel()
            {
                Username = "Nikolcho",
                Count = regionId,
                RegionId = 1, // Eune
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGetMatches");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.GetGamesAsync(input));
        }

        [Fact]
        public async Task GetModelByMatchesShouldReturnListOfHomePageGameViewModel()
        {
            var summonerName = "Nikolcho";
            var count = 2;
            var region = Region.Eune;
            var regionId = 1;

            var summoner = await this.api.Summoner.GetSummonerByNameAsync(region, summonerName);

            var matchList = await this.api.Match.GetMatchListAsync(region, summoner.AccountId);
            var games = new List<RiotSharp.Endpoints.MatchEndpoint.Match>();

            for (int i = 0; i < count; i++)
            {
                var game = await this.api.Match.GetMatchAsync(region, matchList.Matches[i].GameId);

                games.Add(game);
            }

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase("lolSummonerGetMatches");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            var result = (await service.GetModelByMatches(games, regionId)).ToList();

            Assert.NotNull(result);
            Assert.IsType<List<HomePageGameViewModel>>(result);
            Assert.NotNull(result[0].BlueTeam);
            Assert.NotNull(result[0].RedTeam);
            Assert.IsType<TeamDTO>(result[0].BlueTeam);
            Assert.IsType<TeamDTO>(result[0].RedTeam);
            Assert.Equal(games[0].GameId, result[0].GameId);
            Assert.Equal(games[1].GameId, result[1].GameId);
            Assert.Equal(games[0].Participants.First().Stats.Winner, result[0].BlueTeam.State != "Fail");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(12)]
        [InlineData(18)]
        [InlineData(-12313)]
        public async Task GetModelByMatchesShouldThrowArgumentExceptionIfGivenInvalidRegionId(int regionId)
        {
            var games = new List<RiotSharp.Endpoints.MatchEndpoint.Match>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGetMatches");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.GetModelByMatches(games, regionId));
        }

        [Fact]
        public async Task GetModelByMatchesShouldThrowArgumentNullExceptionIfGivenInvalidGameCollection()
        {
            var collectionWithoutGames = new List<RiotSharp.Endpoints.MatchEndpoint.Match>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGetMatches");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GetModelByMatches(null, 2));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.GetModelByMatches(collectionWithoutGames, 2));
        }

        [Fact]
        public async Task GetModelByGameIdShouldReturnHomePageGameViewModelOfGivenGame()
        {
            var riotGameId = 2657118595;
            var regionId = 1;

            var user = new ApplicationUser()
            {
                Email = "f@a.b",
            };
            var game = new Game()
            {
                RiotGameId = riotGameId,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolGetMatcheByGameId");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.Games.AddAsync(game);
            await db.UserGames.AddAsync(new UserGames
            {
                UserId = user.Id,
                GameId = game.GameId,
            });
            await db.SaveChangesAsync();

            var userId = user.Id;

            var expectedGameStats = await this.api.Match.GetMatchAsync(Region.Eune, riotGameId);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            var result = await service.GetModelByGameId(riotGameId, regionId, userId);

            Assert.NotNull(result);
            Assert.NotNull(result.BlueTeam);
            Assert.NotNull(result.RedTeam);
            Assert.IsType<TeamDTO>(result.BlueTeam);
            Assert.IsType<TeamDTO>(result.BlueTeam);
            Assert.IsType<HomePageGameViewModel>(result);
            Assert.Equal(riotGameId, result.GameId);
            Assert.Equal(expectedGameStats.Teams[0].DragonKills, result.BlueTeam.DragonsSlain);
            Assert.Equal(expectedGameStats.Teams[1].DragonKills, result.RedTeam.DragonsSlain);
            Assert.Equal(expectedGameStats.Teams[0].BaronKills, result.BlueTeam.BaronsSlain);
            Assert.Equal(expectedGameStats.Teams[1].BaronKills, result.RedTeam.BaronsSlain);
            Assert.Equal(expectedGameStats.Teams[0].Win, result.BlueTeam.State);
            Assert.Equal(expectedGameStats.Teams[1].Win, result.RedTeam.State);
        }

        [Theory]
        [InlineData(-7)]
        [InlineData(33)]
        [InlineData(11)]
        [InlineData(-4499)]
        public async Task GetModelByGameIdShouldThrowArgumentExceptionIfGivenInvalidRegionId(int regionId)
        {
            var riotGameId = 2657118595;

            var user = new ApplicationUser()
            {
                Email = "f@a.b",
            };
            var game = new Game()
            {
                RiotGameId = riotGameId,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGetMatches");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.Games.AddAsync(game);
            await db.UserGames.AddAsync(new UserGames
            {
                UserId = user.Id,
                GameId = game.GameId,
            });
            await db.SaveChangesAsync();

            var userId = user.Id;

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.GetModelByGameId(riotGameId, regionId, userId));
        }

        [Theory]
        [InlineData(2657118595, 1)]
        [InlineData(2652692459, 1)]
        [InlineData(2655757524, 1)]
        [InlineData(4503146831, 2)]
        public async Task GetModelByGameIdShouldThrowArgumentExceptionIfGameWithThatIdIsNotInDatbase(long gameId, int regionId)
        {
            var user = new ApplicationUser()
            {
                Email = "f@a.b",
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGetMatcheByIdOfGame");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var userId = user.Id;

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.GetModelByGameId(gameId, regionId, userId));
        }

        [Theory]
        [InlineData(2657118595, 1)]
        [InlineData(2652692459, 1)]
        [InlineData(2655757524, 1)]
        [InlineData(4503146831, 2)]
        public async Task GetModelByGameIdShouldThrowInvalidOperationExceptionIfGameWithThatIdIsNotInUsersCollection(long gameId, int regionId)
        {
            var user = new ApplicationUser()
            {
                Email = "f@a.b",
            };
            var game = new Game()
            {
                RiotGameId = gameId,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGetMatcheByGameId");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.Games.AddAsync(game);
            await db.SaveChangesAsync();

            var userId = user.Id;

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.GetModelByGameId(gameId, regionId, userId));
        }

        [Fact]
        public async Task AddGameToCollectionShouldAddGameToDatabase()
        {
            var validGameId = 2657118595;
            int regionId = 1;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolAddGameToCollection");
            var db = new ApplicationDbContext(options.Options);

            RegionsService regionsService = new RegionsService(db);
            await regionsService.UpdateRegions();
            ChampionsService championsService = new ChampionsService(db);
            await championsService.UploadChamionsToDBAsync();

            var expectedGame = await this.api.Match.GetMatchAsync(Region.Eune, validGameId);
            var expectedCollectionGameCount = 1;

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await service.AddGameToCollection(validGameId, regionId);

            var resultGame = await db.Games.FirstOrDefaultAsync();
            var firstPlayer = resultGame.Teams.First().Players.First();
            var lastPlayer = resultGame.Teams.Last().Players.Last();
            var firstPlayerChampion = await db.ChampionsStatic.FirstOrDefaultAsync(c => c.ChampionId == firstPlayer.PlayerChampions.First().ChampionId);
            var lastPlayerChampion = await db.ChampionsStatic.FirstOrDefaultAsync(c => c.ChampionId == lastPlayer.PlayerChampions.First().ChampionId);

            Assert.NotNull(resultGame);
            Assert.NotNull(resultGame.Teams);
            Assert.NotNull(resultGame.Teams[0].Players);
            Assert.NotNull(resultGame.Teams[1].Players);
            Assert.NotNull(resultGame.Teams[0]);
            Assert.NotNull(resultGame.Teams[1]);
            Assert.Equal(expectedCollectionGameCount, await db.Games.CountAsync());
            Assert.Equal(expectedGame.GameId, resultGame.RiotGameId);
            Assert.Equal(expectedGame.Teams.First().Win, resultGame.Teams.First().State);
            Assert.Equal(expectedGame.Teams.Last().Win, resultGame.Teams.Last().State);
            Assert.Equal(expectedGame.ParticipantIdentities.First().Player.SummonerName, firstPlayer.Username);
            Assert.Equal(expectedGame.ParticipantIdentities.Last().Player.SummonerName, lastPlayer.Username);
            Assert.Equal(expectedGame.Participants.First().ChampionId, int.Parse(firstPlayerChampion.ChampionRiotId));
            Assert.Equal(expectedGame.Participants.Last().ChampionId, int.Parse(lastPlayerChampion.ChampionRiotId));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(12)]
        [InlineData(18)]
        [InlineData(-12313)]
        public async Task AddGameToCollectionShouldThrowArgumentExceptionIfGivenInvalidRegionId(int regionId)
        {
            var validGameId = 2657118595;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolSummonerGetMatches");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.AddGameToCollection(validGameId, regionId));
        }

        [Fact]
        public async Task AddGameToUserShouldAddTheGameToGivenUserById()
        {
            var validGameId = 2657118595;
            int regionId = 1;

            var user = new ApplicationUser()
            {
                Email = "f@a.b",
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolAddGameToUserLol");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            RegionsService regionsService = new RegionsService(db);
            await regionsService.UpdateRegions();
            ChampionsService championsService = new ChampionsService(db);
            await championsService.UploadChamionsToDBAsync();

            var userId = user.Id;

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await service.AddGameToCollection(validGameId, regionId);
            var gameId = (await db.Games.FirstOrDefaultAsync(g => g.RiotGameId == validGameId)).GameId;

            await service.AddGameToUser(userId, validGameId);

            var userGames = await db.UserGames.FirstOrDefaultAsync();

            Assert.NotNull(userGames);
            Assert.Equal(userId, userGames.UserId);
            Assert.Equal(gameId, userGames.GameId);
        }

        [Theory]
        [InlineData(-513513)]
        [InlineData(0)]
        [InlineData(31)]
        [InlineData(-114)]
        public async Task AddGameToUserShouldThrowArgumentExceptionIfGivenInvalidGameId(int gameId)
        {
            var user = new ApplicationUser()
            {
                Email = "f@a.b",
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolAddGameToUser");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var userId = user.Id;

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.AddGameToUser(userId, gameId));
        }

        [Theory]
        [InlineData("fwkafeijiawjfei")]
        [InlineData("Some_Rela")]
        [InlineData("fake_id")]
        [InlineData(null)]
        public async Task AddGameToUserShouldThrowArgumentExceptionIfGivenInvalidUserId(string userId)
        {
            var validGameId = 2657118595;
            int regionId = 1;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase("lolAddGameToUser");
            var db = new ApplicationDbContext(options.Options);

            RegionsService regionsService = new RegionsService(db);
            await regionsService.UpdateRegions();
            ChampionsService championsService = new ChampionsService(db);
            await championsService.UploadChamionsToDBAsync();

            var service = new GamesService(db, this.playersService, this.teamsService.Object);
            await service.AddGameToCollection(validGameId, regionId);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.AddGameToUser(userId, validGameId));
        }

        [Fact]
        public async Task GetGameCountShouldReturnCorrectNumberOfGamesInUsersCollection()
        {
            var validGameId = 2657118595;
            var secondGameId = 2652692459;
            int regionId = 1;

            var user = new ApplicationUser()
            {
                Email = "f@a.b",
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolGameCount");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var userId = user.Id;
            int expectedGameCount = 2;

            RegionsService regionsService = new RegionsService(db);
            await regionsService.UpdateRegions();
            ChampionsService championsService = new ChampionsService(db);
            await championsService.UploadChamionsToDBAsync();

            var service = new GamesService(db, this.playersService, this.teamsService.Object);
            await service.AddGameToCollection(validGameId, regionId);
            await service.AddGameToUser(userId, validGameId);
            await service.AddGameToCollection(secondGameId, regionId);
            await service.AddGameToUser(userId, secondGameId);

            var result = service.GetGameCount(userId);

            Assert.Equal(expectedGameCount, result);
        }

        [Theory]
        [InlineData("fwkafeijiawjfei")]
        [InlineData("Some_Rela")]
        [InlineData("fake_id")]
        [InlineData(null)]
        public void GetGamesCountShouldThrowArgumentExceptionIfGivenInvalidUserId(string userId)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolAddGameToUser");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            Assert.Throws<ArgumentException>(() => service.GetGameCount(userId));
        }

        [Fact]
        public async Task GetCollectionGamesShouldReturnCollectionPageGameViewModel()
        {
            var validGameId = 2660892488;
            int regionId = 1;

            var user = new ApplicationUser()
            {
                Email = "f@a.b",
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolAddGameToUser");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            RegionsService regionsService = new RegionsService(db);
            await regionsService.UpdateRegions();
            ChampionsService championsService = new ChampionsService(db);
            await championsService.UploadChamionsToDBAsync();

            int expectedGameCount = 1;
            var userId = user.Id;

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await service.AddGameToCollection(validGameId, regionId);
            var expectedGame = await db.Games.FirstOrDefaultAsync(g => g.RiotGameId == validGameId);
            await service.AddGameToUser(userId, validGameId);

            var result = (await service.GetCollectionGames(userId)).ToList();
            var resultFirstGame = result.FirstOrDefault();

            Assert.NotNull(result);
            Assert.NotNull(resultFirstGame);
            Assert.IsType<List<CollectionPageGameViewModel>>(result);
            Assert.IsType<CollectionPageGameViewModel>(resultFirstGame);
            Assert.Equal(expectedGameCount, result.Count());
            Assert.Equal(expectedGame.RiotGameId, resultFirstGame.GameId);
            Assert.Equal(expectedGame.Teams.First().State, resultFirstGame.BlueTeam.State);
            Assert.Equal(expectedGame.Teams.Last().State, resultFirstGame.RedTeam.State);
            Assert.Equal(expectedGame.Teams[0].Players.First().Username, resultFirstGame.BlueTeam.Players.First().Username);
            Assert.Equal(expectedGame.Teams[1].Players.Last().Username, resultFirstGame.RedTeam.Players.Last().Username);
        }

        [Theory]
        [InlineData("1=1")]
        [InlineData("Real_Username")]
        [InlineData("fake_id")]
        [InlineData(null)]
        public async Task GetCollectionGamesShouldThrowArgumentExceptionIfGivenInvalidUserId(string userId)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolAddGameToUser");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.GetCollectionGames(userId));
        }

        [Fact]
        public async Task RemoveGameFromCollectionShouldRemoveTheGameFromUsersCollection()
        {
            var validGameId = 2660892488;
            int regionId = 1;

            var user = new ApplicationUser()
            {
                Email = "f@a.b",
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("lolRemoveGameToUser");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            RegionsService regionsService = new RegionsService(db);
            await regionsService.UpdateRegions();
            ChampionsService championsService = new ChampionsService(db);
            await championsService.UploadChamionsToDBAsync();

            int expectedGameCount = 0;
            var userId = user.Id;

            var service = new GamesService(db, this.playersService, this.teamsService.Object);
            await service.AddGameToCollection(validGameId, regionId);
            await service.AddGameToUser(userId, validGameId);

            await service.RemoveGameFromCollection(userId, validGameId);
            var tryGetGame = await db.Games.FirstOrDefaultAsync(g => g.RiotGameId == validGameId);

            Assert.Null(tryGetGame);
            Assert.Equal(expectedGameCount, db.Games.Count());
            Assert.Equal(expectedGameCount, db.UserGames.Count());
        }

        [Theory]
        [InlineData("1=1")]
        [InlineData("Real_Username")]
        [InlineData("fake_id")]
        [InlineData(null)]
        public async Task RemoveGameFromCollectionShouldThrowArgumentExceptionIfGivenInvalidUserId(string userId)
        {
            var validGameId = 2660892488;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseInMemoryDatabase("lolRemoveGameToUser");
            var db = new ApplicationDbContext(options.Options);

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.RemoveGameFromCollection(userId, validGameId));
        }

        [Theory]
        [InlineData(21)]
        [InlineData(-20)]
        [InlineData(41312312)]
        [InlineData(0)]
        public async Task RemoveGameFromCollectionShouldThrowArgumentExceptionIfGivenInvalidGameId(int gameId)
        {
            var user = new ApplicationUser()
            {
                Email = "e@a.c",
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseInMemoryDatabase("lolRemoveGameToUser");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            string userId = user.Id;

            var service = new GamesService(db, this.playersService, this.teamsService.Object);

            await Assert.ThrowsAsync<ArgumentException>(async () => await service.RemoveGameFromCollection(userId, gameId));
        }
    }
}
