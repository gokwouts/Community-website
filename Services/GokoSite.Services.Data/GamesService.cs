namespace GokoSite.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Data;
    using GokoSite.Data.Models.LoL;
    using GokoSite.Services.Data.StaticData;
    using GokoSite.Web.ViewModels.Games;
    using GokoSite.Web.ViewModels.Games.DTOs;
    using Microsoft.EntityFrameworkCore;
    using RiotSharp;
    using RiotSharp.Endpoints.MatchEndpoint;
    using RiotSharp.Endpoints.SummonerEndpoint;

    public class GamesService : IGamesService
    {
        private readonly ApplicationDbContext db;
        private readonly IPlayersService playersService;
        private readonly ITeamsService teamsService;

        public RiotApi Api { get; set; }

        public GamesService(
            ApplicationDbContext db,
            IPlayersService playersService,
            ITeamsService teamsService)
        {
            this.db = db;
            this.playersService = playersService;
            this.teamsService = teamsService;
            this.Api = RiotApi.GetDevelopmentInstance(PublicData.apiKey);
        }

        public async Task<Summoner> GetBasicSummonerDataAsync(string summonerName, RiotSharp.Misc.Region region)
        {
            try
            {
                var summoner = await this.Api.Summoner.GetSummonerByNameAsync(region, summonerName);

                return summoner;
            }
            catch (RiotSharpException ex)
            {
                return null;
            }
        }

        public async Task<Match> GetGameAsync(long gameId, RiotSharp.Misc.Region region)
        {
            try
            {
                var game = await this.Api.Match.GetMatchAsync(region, gameId);

                return game;
            }
            catch (RiotSharpException ex)
            {
                return null;
            }
        }

        public async Task<ICollection<Match>> GetGamesAsync(GetGamesInputModel input)
        {
            if (input.Count < 0 || input.Count > 10)
            {
                throw new ArgumentException("Count must be between 0 and 10!");
            }

            if (input.RegionId < 0 || input.RegionId > 10)
            {
                throw new ArgumentException("Region Id must be between 0 and 10!");
            }

            if (input.Username.Length <= 0 || input.Username.Length > 16) // Leagues max name length is 16!
            {
                throw new ArgumentException("Username must be between 1 and 16 characters long!");
            }

            RiotSharp.Misc.Region region = (RiotSharp.Misc.Region)input.RegionId;

            var summoner = await this.GetBasicSummonerDataAsync(input.Username, region);

            if (summoner == null)
            {
                throw new ArgumentNullException("summonerName", "Wrong summoner name!");
            }

            var matches = await this.Api.Match.GetMatchListAsync(region, summoner.AccountId);

            var games = new List<Match>();

            for (int i = 0; i < input.Count; i++)
            {
                var game = await this.GetGameAsync(matches.Matches[i].GameId, region);

                games.Add(game);
            }

            return games;
        }

        public async Task<IEnumerable<HomePageGameViewModel>> GetModelByMatches(ICollection<Match> games, int regionId)
        {
            if (regionId < 0 || regionId > 10)
            {
                throw new ArgumentException("Region Id must be between 0 and 10!");
            }

            if (games == null || games?.Count == 0)
            {
                throw new ArgumentNullException("games", "Games must have games in it!");
            }

            var viewModel = new List<HomePageGameViewModel>();

            foreach (var game in games)
            {
                viewModel.Add(new HomePageGameViewModel
                {
                    GameId = game.GameId,
                    RegionId = regionId,
                    BlueTeam = new TeamDTO
                    {
                        Players = await this.playersService.GetPlayersByParticipantsDto(game.ParticipantIdentities, game.Participants, 100),
                        State = game.Teams[0].Win,
                    },
                    RedTeam = new TeamDTO
                    {
                        Players = await this.playersService.GetPlayersByParticipantsDto(game.ParticipantIdentities, game.Participants, 200),
                        State = game.Teams[1].Win,
                    },
                });
            }

            return viewModel.ToList();
        }

        public async Task<HomePageGameViewModel> GetModelByGameId(long gameId, int regionId, string userId) // More eficient..
        {
            if (regionId < 0 || regionId > 10)
            {
                throw new ArgumentException("Region Id must be between 0 and 10!");
            }

            var region = (RiotSharp.Misc.Region)regionId;
            var game = await this.Api.Match.GetMatchAsync(region, gameId);

            var dbGame = this.db.Games.FirstOrDefault(g => g.RiotGameId == game.GameId);

            if (dbGame == null)
            {
                throw new ArgumentException("Game with that Game Id does not exist in the database!");
            }

            var isInUserCollection = this.db.UserGames.Any(ug => ug.UserId == userId && ug.GameId == dbGame.GameId);

            if (isInUserCollection == false && userId != "SharedGameUser")
            {
                throw new InvalidOperationException($"This user ({userId}) does not contain a game with id({gameId}) in his collection!");
            }

            var viewModel = new HomePageGameViewModel
            {
                GameId = game.GameId,
                RegionId = regionId,
                BlueTeam = new TeamDTO
                {
                    Players = await this.playersService.GetPlayersByParticipantsDto(game.ParticipantIdentities, game.Participants, 100),
                    State = game.Teams[0].Win,
                    DragonsSlain = game.Teams[0].DragonKills,
                    BaronsSlain = game.Teams[0].BaronKills,
                    TurretsDestroyed = game.Teams[0].TowerKills,
                    TotalGold = this.teamsService.GetTotalGoldByPlayers(game.ParticipantIdentities, game.Participants, 100),
                },
                RedTeam = new TeamDTO
                {
                    Players = await this.playersService.GetPlayersByParticipantsDto(game.ParticipantIdentities, game.Participants, 200),
                    State = game.Teams[1].Win,
                    DragonsSlain = game.Teams[1].DragonKills,
                    BaronsSlain = game.Teams[1].BaronKills,
                    TurretsDestroyed = game.Teams[1].TowerKills,
                    TotalGold = this.teamsService.GetTotalGoldByPlayers(game.ParticipantIdentities, game.Participants, 200),
                },
            };

            return viewModel;
        }

        public async Task AddGameToCollection(long gameId, int regionId)
        {
            if (regionId < 0 || regionId > 10)
            {
                throw new ArgumentException("Region Id must be between 0 and 10!");
            }

            var curGame = await this.GetGameAsync(gameId, (RiotSharp.Misc.Region)regionId);

            var region = await this.db.Regions.FirstOrDefaultAsync(r => r.RiotRegionId == regionId);

            var game = new Game()
            {
                Region = region,
                RegionId = region.RegionId,
                RiotGameId = gameId,
            };

            var firstTeam = new Team
            {
                State = curGame.Teams[0].Win,
            };

            var secondTeam = new Team
            {
                State = curGame.Teams[1].Win,
            };

            game.Teams.Add(firstTeam);
            game.Teams.Add(secondTeam);

            await this.db.Games.AddAsync(game);
            await this.db.SaveChangesAsync();

            var firstTeamPlayers = this.playersService.GetPlayersByParticipants(curGame.ParticipantIdentities, curGame.Participants, 100).ToList();
            // 100 first team / 200 second team
            var secondTeamPlayers = this.playersService.GetPlayersByParticipants(curGame.ParticipantIdentities, curGame.Participants, 200).ToList();

            firstTeamPlayers.ForEach(p => game.Teams[0].Players.Add(p));
            secondTeamPlayers.ForEach(p => game.Teams[1].Players.Add(p));

            await this.db.SaveChangesAsync();

            var dbGame = await this.db.Games.OrderByDescending(g => g.GameId).FirstOrDefaultAsync();

            var firstTeamId = dbGame.Teams[0].TeamId;
            var secondTeamId = dbGame.Teams[1].TeamId;

            var players = await this.db.Players.Where(p => p.TeamId == firstTeamId || p.TeamId == secondTeamId).Select(p => p).ToListAsync();
            var champions = await this.db.ChampionsStatic.ToListAsync();

            for (int i = 0; i < players.Count; i++)
            {
                var playerId = players[i].PlayerId;
                var participantIndex = curGame.ParticipantIdentities.FirstOrDefault(p => p.Player.SummonerName == players[i].Username).ParticipantId - 1;
                var champRiotId = curGame.Participants[participantIndex].ChampionId;
                var champId = champions.FirstOrDefault(c => c.ChampionRiotId == champRiotId.ToString()).ChampionId;

                await this.db.PlayerChampion.AddAsync(new PlayerChampion
                {
                    PlayerId = playerId,
                    ChampionId = champId,
                });
            }

            await this.db.SaveChangesAsync();
        }

        public async Task AddGameToUser(string userId, long riotGameId)
        {
            var dbGame = await this.db.Games.OrderByDescending(g => g.GameId)
                .FirstOrDefaultAsync(g => g.RiotGameId == riotGameId);
            var user = await this.db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (dbGame == null)
            {
                throw new ArgumentException("There is no game with the given id!");
            }

            if (user == null)
            {
                throw new ArgumentException("There is no user with the given id!");
            }

            await this.db.UserGames.AddAsync(new UserGames
            {
                UserId = user.Id,
                GameId = dbGame.GameId,
            });
            await this.db.SaveChangesAsync();
        }

        public int GetGameCount(string userId)
        {
            var user = this.db.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
            {
                throw new ArgumentException("There is no user with the given id!");
            }

            return this.db.UserGames.Where(u => u.UserId == userId).Count();
        }

        public async Task<ICollection<CollectionPageGameViewModel>> GetCollectionGames(string userId)
        {
            var user = await this.db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ArgumentException("There is no user with the given id!");
            }

            var viewModel = new List<CollectionPageGameViewModel>();
            var gameIds = await this.db.UserGames
                .Where(ug => ug.UserId == user.Id)
                .Select(ug => new { ug.GameId })
                .ToListAsync();

            foreach (var gameId in gameIds)
            {
                var curGame = await this.db.Games.FirstOrDefaultAsync(g => g.GameId == gameId.GameId);

                var curGameTeams = await this.db.Teams
                    .Where(t => t.GameId == curGame.GameId)
                    .ToArrayAsync();

                // first team
                Team fTeam = curGameTeams[0];
                List<GokoSite.Data.Models.LoL.Player> fPlayers = await this.db.Players
                    .Where(p => p.TeamId == fTeam.TeamId)
                    .ToListAsync();

                List<Champion> fChampions = new List<Champion>();

                foreach (var player in fPlayers)
                {
                    fChampions.Add(await this.db.PlayerChampion
                    .Where(pc => pc.PlayerId == player.PlayerId)
                    .Select(pc => new Champion
                    {
                        ChampionIconUrl = pc.Champion.ChampionIconUrl,
                        ChampionName = pc.Champion.ChampionName,
                        ChampionRiotId = pc.Champion.ChampionRiotId,
                        ChampionId = pc.Champion.ChampionId,
                    })
                    .FirstOrDefaultAsync());
                }

                // second team
                Team sTeam = curGameTeams[1];
                List<GokoSite.Data.Models.LoL.Player> sPlayers = await this.db.Players
                    .Where(p => p.TeamId == sTeam.TeamId)
                    .ToListAsync();

                List<Champion> sChampions = new List<Champion>();

                foreach (var player in sPlayers)
                {
                    sChampions.Add(await this.db.PlayerChampion
                    .Where(pc => pc.PlayerId == player.PlayerId)
                    .Select(pc => new Champion
                    {
                        ChampionIconUrl = pc.Champion.ChampionIconUrl,
                        ChampionName = pc.Champion.ChampionName,
                        ChampionRiotId = pc.Champion.ChampionRiotId,
                        ChampionId = pc.Champion.ChampionId,
                    })
                    .FirstOrDefaultAsync());
                }

                viewModel.Add(this.GetModelByGame(curGame, fChampions, sChampions));
            }

            return viewModel;
        }

        public async Task RemoveGameFromCollection(string userId, long gameId)
        {
            var user = await this.db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new ArgumentException("There is no user with the given id!");
            }

            var game = await this.db.Games
                .FirstOrDefaultAsync(g => g.RiotGameId == gameId);

            if (game == null)
            {
                throw new ArgumentException("There is not game with the given id");
            }

            var teams = await this.db.Teams
                .Where(t => t.GameId == game.GameId)
                .ToArrayAsync();

            // first team
            var fTeam = teams[0];
            var fPlayers = await this.db.Players
                .Where(p => p.TeamId == fTeam.TeamId)
                .ToListAsync();

            fPlayers.ForEach(p => p.PlayerChampions.Clear());
            this.db.Players.RemoveRange(fPlayers);

            // second team
            var sTeam = teams[1];
            var sPlayers = await this.db.Players
                .Where(p => p.TeamId == sTeam.TeamId)
                .ToListAsync();

            sPlayers.ForEach(p => p.PlayerChampions.Clear());
            this.db.Players.RemoveRange(sPlayers);

            this.db.Teams.RemoveRange(teams);

            (await this.db.Games.FirstOrDefaultAsync(g => g.RiotGameId == gameId)).UserGames.Clear();

            this.db.Games.Remove(game);

            await this.db.SaveChangesAsync();
        }

        private CollectionPageGameViewModel GetModelByGame(Game game, List<Champion> fChampions, List<Champion> sChampions)
        {
            var regionId = this.db.Regions.FirstOrDefault(r => r.RegionId == game.RegionId).RiotRegionId;

            var curModel = new CollectionPageGameViewModel
            {
                GameId = game.RiotGameId,
                RegionId = regionId,
                BlueTeam = new TeamDTO
                {
                    Players = GetPlayersDtoList(game.Teams[0].Players, fChampions),
                    State = game.Teams[0].State,
                },
                RedTeam = new TeamDTO
                {
                    Players = GetPlayersDtoList(game.Teams[1].Players, sChampions),
                    State = game.Teams[1].State,
                },
            };

            return curModel;
        }

        private List<PlayerDTO> GetPlayersDtoList(ICollection<GokoSite.Data.Models.LoL.Player> players, List<Champion> champions)
        {
            var dtos = new List<PlayerDTO>();

            int i = 0;
            foreach (var player in players)
            {
                var champion = champions[i];

                dtos.Add(new PlayerDTO
                {
                    Username = player.Username,
                    ProfileIconUrl = player.ProfileIconUrl,
                    Champion = new ChampionDTO
                    {
                        ChampionIconUrl = champion.ChampionIconUrl,
                        ChampionName = champion.ChampionName,
                    },
                });
                i++;
            }

            return dtos;
        }
    }
}
