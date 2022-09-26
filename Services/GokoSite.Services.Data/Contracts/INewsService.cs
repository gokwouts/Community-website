namespace GokoSite.Services.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using GokoSite.Web.ViewModels.News;

    public interface INewsService
    {
        public ICollection<NewHomePageViewModel> GetNews();

        public NewDetailsPageViewModel GetNew(string newId);

        public Task AddNew(NewAddInputModel input, string userId);

        public Task<bool> RemoveNew(string newId);

        public Task<bool> EditNew(NewAddInputModel input, string newId);
    }
}
