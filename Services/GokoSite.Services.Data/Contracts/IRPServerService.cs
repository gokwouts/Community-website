namespace GokoSite.Services.Data
{
    using GokoSite.Web.ViewModels.Players;

    public interface IRPServerService
    {
        public HomePageViewModel GetPlayers();
    }
}
