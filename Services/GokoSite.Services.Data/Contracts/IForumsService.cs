namespace GokoSite.Services.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using GokoSite.Web.ViewModels.Forums;

    // TODO
    public interface IForumsService
    {
        public Task<ICollection<ForumViewModel>> GetAll(string userId);

        public ICollection<PersonalForumViewModel> GetPersonalPosts(string userId);

        public EditForumViewModel GetPost(string postId);

        public Task DeletePost(string postId);

        public Task EditPost(EditForumInputModel input);

        public Task Like(string postId, string userId);

        public Task<string> AddPost(AddForumModel input);

        public Task AddPostToUser(string userId, string forumId);
    }
}
