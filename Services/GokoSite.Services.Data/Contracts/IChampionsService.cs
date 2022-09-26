namespace GokoSite.Services.Data
{
    using System.Threading.Tasks;

    using GokoSite.Data.Models.LoL;
    using GokoSite.Web.ViewModels.Games.DTOs;

    public interface IChampionsService
    {
        Task<Champion> GetChampion(int championId);

        Task<ChampionDTO> GetChampionDto(int championId);

    }
}
