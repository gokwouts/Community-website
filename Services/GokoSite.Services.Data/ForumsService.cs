namespace GokoSite.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Data;
    using GokoSite.Data.Models.RP;
    using GokoSite.Web.ViewModels.Forums;
    using Microsoft.EntityFrameworkCore;

    public class ForumsService : IForumsService
    {
        private readonly ApplicationDbContext db;

        public ForumsService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<string> AddPost(AddForumModel input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input", "The given input is null!");
            }

            var post = new Forum()
            {
                ForumText = input.Text,
                ForumTopic = input.Topic,
                Likes = 0,
            };

            await this.db.Forums.AddAsync(post);

            await this.db.SaveChangesAsync();

            return post.ForumId;
        }

        public async Task AddPostToUser(string userId, string forumId)
        {
            if (string.IsNullOrEmpty(forumId))
            {
                throw new ArgumentNullException("forumId", "The given forum Id is null or emtpy string!");
            }

            var post = await this.db.Forums.FirstOrDefaultAsync(f => f.ForumId == forumId);

            if (post == null)
            {
                throw new ArgumentNullException("forumId", "The given forum Id not valid!");
            }

            await this.db.UserForums.AddAsync(new UserForums()
            {
                UserId = userId,
                ForumId = post.ForumId,
            });

            await this.db.SaveChangesAsync();
        }

        public async Task DeletePost(string postId)
        {
            var post = await this.db.Forums.FirstOrDefaultAsync(f => f.ForumId == postId);

            if (post == null)
            {
                throw new InvalidOperationException("No post found with the given post Id!");
            }

            this.db.Remove(post);
            await this.db.SaveChangesAsync();
        }

        public async Task EditPost(EditForumInputModel input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input", "The given input is null!");
            }

            var post = await this.db.Forums.FirstOrDefaultAsync(f => f.ForumId == input.ForumId);

            if (post == null)
            {
                throw new InvalidOperationException("No post found with this id!");
            }

            post.ForumText = input.Text;
            post.ForumTopic = input.Topic;

            await this.db.SaveChangesAsync();
        }

        public async Task<ICollection<ForumViewModel>> GetAll(string userId)
        {
            var forums = new List<ForumViewModel>();
            foreach (var forum in this.db.Forums.ToList())
            {
                forums.Add(new ForumViewModel()
                {
                    ForumId = forum.ForumId,
                    ForumText = forum.ForumText,
                    ForumTopic = forum.ForumTopic,
                    Likes = forum.Likes,
                    Liked = await this.IsForumLiked(forum.ForumId, userId),
                });
            }
            forums = forums.OrderByDescending(f => f.Likes).ToList();
            return forums;
        }

        public ICollection<PersonalForumViewModel> GetPersonalPosts(string userId)
        {
            var personalForums = new List<PersonalForumViewModel>();

            var userForums = this.db.UserForums.Where(uf => uf.UserId == userId).ToList();
            var forumIds = userForums.Select(uf => uf.ForumId).ToArray();

            foreach (var forum in this.db.Forums.Where(f => forumIds.Contains(f.ForumId)))
            {
                personalForums.Add(new PersonalForumViewModel()
                {
                    ForumId = forum.ForumId,
                    ForumText = forum.ForumText,
                    ForumTopic = forum.ForumTopic,
                    OwnerId = userId,
                });
            }

            return personalForums;
        }

        public EditForumViewModel GetPost(string postId)
        {
            var postDb = this.db.Forums.FirstOrDefault(f => f.ForumId == postId);

            if (postDb == null)
            {
                throw new ArgumentNullException("postId", "No post found with this id!");
            }

            var userForum = this.db.UserForums.FirstOrDefault(uf => uf.ForumId == postId);

            if (userForum == null)
            {
                throw new ArgumentException("No owner found for this post!");
            }

            var owner = this.db.Users.FirstOrDefault(u => u.Id == userForum.UserId);

            return new EditForumViewModel()
            {
                ForumId = postDb.ForumId,
                Topic = postDb.ForumTopic,
                Text = postDb.ForumText,
                Likes = postDb.Likes,
                OwnerName = owner.UserName,
            };
        }

        public async Task Like(string postId, string userId)
        {
            var forum = await this.db.Forums.FirstOrDefaultAsync(f => f.ForumId == postId);

            if (forum == null)
            {
                throw new ArgumentNullException("postId", "No post found with this id!");
            }

            var isLiked = await this.IsForumLiked(postId, userId);
            var userLike = await this.db.UserLikes.FirstOrDefaultAsync(f => f.ForumId == postId && f.UserId == userId);

            if (isLiked)
            {
                forum.Likes -= 1;
                userLike.Liked = false;
            }
            else
            {
                forum.Likes += 1;
                userLike.Liked = true;
            }

            await this.db.SaveChangesAsync();
        }

        private async Task<bool> IsForumLiked(string postId, string userId)
        {
            var userLike = await this.db.UserLikes.FirstOrDefaultAsync(f => f.ForumId == postId && f.UserId == userId);

            if (userLike == null)
            {
                this.db.UserLikes.Add(new UserLikes()
                {
                    ForumId = postId,
                    UserId = userId,
                    Liked = false,
                });

                await this.db.SaveChangesAsync();

                return false;
            }

            return userLike.Liked;
        }
    }
}
