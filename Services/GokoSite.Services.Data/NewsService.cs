namespace GokoSite.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Data;
    using GokoSite.Data.Models.News;
    using GokoSite.Web.ViewModels.News;

    public class NewsService : INewsService
    {
        private readonly ApplicationDbContext db;

        public NewsService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task AddNew(NewAddInputModel input, string userId)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input", $"The given input was null!");
            }

            if (!this.db.Users.Any(u => u.Id == userId))
            {
                throw new ArgumentNullException("userId", $"There is no user with the given user Id!");
            }

            var newDb = new New()
            {
                Title = input.Title,
                Content = input.Content,
                Image = input.Image,
                UserId = userId,
                UploadedOn = DateTime.Now,
            };

            await this.db.News.AddAsync(newDb);

            await this.db.SaveChangesAsync();
        }

        public async Task<bool> EditNew(NewAddInputModel input, string newId)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input", $"The given input was null!");
            }

            var newDb = this.db.News.FirstOrDefault(n => n.NewId == newId);

            if (newDb != null)
            {
                newDb.Title = input.Title;
                newDb.Content = input.Content;
                newDb.Image = input.Image;

                await this.db.SaveChangesAsync();

                return true;
            }
            else
            {
                throw new ArgumentNullException("newId", $"There is no new with the given new Id ({newId})");
            }
        }

        public ICollection<NewHomePageViewModel> GetNews()
        {
            var news = this.db.News.Select(n => new NewHomePageViewModel()
            {
                NewId = n.NewId,
                Title = n.Title,
                Image = n.Image,
            }).ToList();

            return news;
        }

        public async Task<bool> RemoveNew(string newId)
        {
            var newDb = this.db.News.FirstOrDefault(n => n.NewId == newId);

            if (newDb != null)
            {
                this.db.News.Remove(newDb);
                await this.db.SaveChangesAsync();

                return true;
            }
            else
            {
                throw new ArgumentNullException("newId", $"There is no new with the given new Id ({newId})");
            }
        }

        public NewDetailsPageViewModel GetNew(string newId)
        {
            var newDb = this.db.News.FirstOrDefault(n => n.NewId == newId);

            if (newDb == null)
            {
                throw new ArgumentNullException("newId", $"There is no new with the given new Id ({newId})");
            }

            var user = this.db.Users.FirstOrDefault(u => u.Id == newDb.UserId);

            if (user == null)
            {
                throw new ArgumentNullException("user", $"This new does not belong to any user!");
            }

            var newModel = new NewDetailsPageViewModel()
            {
                NewId = newId,
                Title = newDb.Title,
                Content = newDb.Content,
                Image = newDb.Image,
                AuthorUsername = user?.UserName,
                UploadedOn = newDb.UploadedOn,
            };

            return newModel;
        }
    }
}
