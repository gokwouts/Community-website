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

    public class ForumsServiceTests
    {
        [Fact]
        public async Task AddPostShouldAddAPostWithTheGivenInputModelAndReturnPostId()
        {
            int defaultLikes = 0;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTest");
            var db = new ApplicationDbContext(options.Options);

            var topic = "TestTopic";
            var text = "TestText";

            var newInputAdd = new AddForumModel()
            {
                Topic = topic,
                Text = text,
            };

            var countBeforeAdding = db.Forums.Count();
            var service = new ForumsService(db);

            var postId = await service.AddPost(newInputAdd);
            var postDb = db.Forums.FirstOrDefault(f => f.ForumId == postId);

            Assert.Equal(countBeforeAdding + 1, db.Forums.Count());
            Assert.NotNull(postDb);
            Assert.Equal(defaultLikes, postDb.Likes);
        }

        [Fact]
        public async Task AddPostShouldThrowArgumentNullExceptionIfNotGivenInput()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTest");
            var db = new ApplicationDbContext(options.Options);

            var service = new ForumsService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.AddPost(null));
        }

        [Fact]
        public async Task AddPostToUserShouldAddTheGivenForumIdToTheGivenUserWithId()
        {
            int defaultLikes = 0;
            var topic = "TestTopic";
            var text = "TestText";

            var forum = new Forum()
            {
                ForumText = text,
                ForumTopic = topic,
                Likes = defaultLikes,
            };

            var user = new ApplicationUser()
            {
                Email = "test.test@test.testing",
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTestAddPost");
            var db = new ApplicationDbContext(options.Options);

            await db.Forums.AddAsync(forum);
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var service = new ForumsService(db);

            int countBeforeAdd = db.UserForums.Count();

            await service.AddPostToUser(user.Id, forum.ForumId);

            var uf = db.UserForums.FirstOrDefault(uf => uf.ForumId == forum.ForumId && uf.UserId == user.Id);

            Assert.NotNull(uf);
            Assert.Equal(countBeforeAdd + 1, db.UserForums.Count());
        }

        [Fact]
        public async Task AddPostToUserShouldThrowArgumentNullExceptionIfGivenForumIdIsNullOrEmpty()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTestAddPost");
            var db = new ApplicationDbContext(options.Options);

            var service = new ForumsService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.AddPostToUser("real_user_id", null));
        }

        [Fact]
        public async Task AddPostToUserShouldThrowArgumentNullExceptionIfGivenForumIdIsInvalid()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTestAddPost");
            var db = new ApplicationDbContext(options.Options);

            var service = new ForumsService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.AddPostToUser("real_user_id", "fake_forum_id"));
        }

        [Fact]
        public async Task DeletePostShouldRemoveThePostIfGivenValidForumId()
        {
            int defaultLikes = 0;
            var topic = "TestTopic";
            var text = "TestText";

            var forum = new Forum()
            {
                ForumText = text,
                ForumTopic = topic,
                Likes = defaultLikes,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTestAddPost");
            var db = new ApplicationDbContext(options.Options);

            await db.Forums.AddAsync(forum);
            await db.SaveChangesAsync();

            int countBeforeDelete = db.Forums.Count();
            var service = new ForumsService(db);

            await service.DeletePost(forum.ForumId);

            var forumDb = db.Forums.FirstOrDefault(f => f.ForumId == forum.ForumId);

            Assert.Null(forumDb);
            Assert.Equal(countBeforeDelete - 1, db.Forums.Count());
        }

        [Fact]
        public async Task DeletePostShouldThrowInvalidOperationExceptionIfGivenInvalidForumId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTestAddPost");
            var db = new ApplicationDbContext(options.Options);

            var service = new ForumsService(db);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.DeletePost("fake_forum_id"));
        }

        [Fact]
        public async Task EditPostShouldEditTheGivenPostWithTheGivenEditForumInputModel()
        {
            int defaultLikes = 0;
            var topic = "TestTopic";
            var text = "TestText";

            var forum = new Forum()
            {
                ForumText = text,
                ForumTopic = topic,
                Likes = defaultLikes,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTestAddPost");
            var db = new ApplicationDbContext(options.Options);

            await db.Forums.AddAsync(forum);
            await db.SaveChangesAsync();

            var service = new ForumsService(db);

            string editedTopic = "EditedTopic";
            string editedText = "EditedText";

            var editInput = new EditForumInputModel()
            {
                ForumId = forum.ForumId,
                Topic = editedTopic,
                Text = editedText,
            };

            await service.EditPost(editInput);

            var forumDb = await db.Forums.FirstOrDefaultAsync(f => f.ForumId == forum.ForumId);

            Assert.NotNull(forumDb);
            Assert.Equal(editedTopic, forumDb.ForumTopic);
            Assert.Equal(editedText, forumDb.ForumText);
        }

        [Fact]
        public async Task EditPostShouldThrowArgumentNullExceptionIfNotGivenInput()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTestAddPost");
            var db = new ApplicationDbContext(options.Options);

            var service = new ForumsService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.EditPost(null));
        }

        [Fact]
        public async Task EditPostShouldThrowInvalidOperationExceptionIfGivenInvalidForumId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTestAddPost");
            var db = new ApplicationDbContext(options.Options);

            var service = new ForumsService(db);

            string editedTopic = "EditedTopic";
            string editedText = "EditedText";

            var editInput = new EditForumInputModel()
            {
                ForumId = "fake_forum_id",
                Topic = editedTopic,
                Text = editedText,
            };

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.EditPost(editInput));
        }

        [Fact]
        public async Task GetAllShouldReturnCollectionOfForumViewModelOfAllForumsInDatase()
        {
            var user = new ApplicationUser()
            {
                Email = "TE.st@te.sst",
            };

            var topic = "TestTopic";
            var text = "TestText";

            var newInputAddF = new Forum()
            {
                ForumTopic = topic,
                ForumText = text,
            };

            var newInputAddS = new Forum()
            {
                ForumTopic = topic,
                ForumText = text,
            };

            var forums = new List<Forum>();
            forums.Add(newInputAddF);
            forums.Add(newInputAddS);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase("forumTestGetPostsAll");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.Forums.AddRangeAsync(forums);
            await db.SaveChangesAsync();

            int expectedCount = forums.Count();

            var service = new ForumsService(db);

            var result = await service.GetAll(user.Id);

            Assert.NotNull(result);
            Assert.Equal(expectedCount, result.Count());
            Assert.IsType<ForumViewModel>(result.First());
        }

        [Fact]
        public async Task GetPersonalPostsShouldReturnCollectionOfAllForumPostsMadeByTheUser()
        {
            var user = new ApplicationUser()
            {
                Email = "TE.st@te.sst",
            };
            var topic = "TestTopic";
            var text = "TestText";
            var newInputAddF = new Forum()
            {
                ForumTopic = topic,
                ForumText = text,
            };
            var newInputAddS = new Forum()
            {
                ForumTopic = topic,
                ForumText = text,
            };

            var forums = new List<Forum>();
            forums.Add(newInputAddF);
            forums.Add(newInputAddS);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase("forumTestGetPosts");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.Forums.AddRangeAsync(forums);
            await db.SaveChangesAsync();

            int expectedCount = forums.Count();

            var service = new ForumsService(db);

            // Setup

            await service.AddPostToUser(user.Id, newInputAddF.ForumId);
            await service.AddPostToUser(user.Id, newInputAddS.ForumId);

            // Testing

            var result = service.GetPersonalPosts(user.Id);

            Assert.NotNull(result);
            Assert.Equal(expectedCount, result.Count());
            Assert.IsType<PersonalForumViewModel>(result.First());
        }

        [Fact]
        public async Task GetPostShouldReturnThePostWithTheGivenId()
        {
            var user = new ApplicationUser()
            {
                Email = "afs@.fa.a",
                UserName = "afs@.fa.a",
            };
            var topic = "TestTopic";
            var text = "TestText";
            var newForum = new Forum()
            {
                ForumTopic = topic,
                ForumText = text,
            };
            var userForum = new UserForums()
            {
                ForumId = newForum.ForumId,
                Forum = newForum,
                User = user,
                UserId = user.Id
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTestGetPostNew");
            var db = new ApplicationDbContext(options.Options);

            await db.Forums.AddAsync(newForum);
            await db.Users.AddAsync(user);
            await db.UserForums.AddAsync(userForum);
            await db.SaveChangesAsync();

            var service = new ForumsService(db);

            var result = service.GetPost(newForum.ForumId);

            Assert.NotNull(result);
            Assert.IsType<EditForumViewModel>(result);
            Assert.Equal(topic, result.Topic);
            Assert.Equal(text, result.Text);
            Assert.Equal(user.UserName, result.OwnerName);
        }

        [Fact]
        public async Task GetPostShouldThrowArgumentExceptionIfPostDoesntBelongToUser()
        {
            var topic = "TestTopic";
            var text = "TestText";
            var newForum = new Forum()
            {
                ForumTopic = topic,
                ForumText = text,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("forumTestGetPostNew");
            var db = new ApplicationDbContext(options.Options);

            await db.Forums.AddAsync(newForum);
            await db.SaveChangesAsync();

            var service = new ForumsService(db);

            Assert.Throws<ArgumentException>(() => service.GetPost(newForum.ForumId));
        }

        [Fact]
        public void GetPostShouldThrowArgumentNullExceptionIfGivenInvalidId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
.UseInMemoryDatabase("forumTestGetPost");
            var db = new ApplicationDbContext(options.Options);

            var service = new ForumsService(db);

            Assert.Throws<ArgumentNullException>(() => service.GetPost("fake_post_id"));
        }

        [Fact]
        public async Task LikeShouldAddALikeToTheGivenPostFromTheGivenUser()
        {
            var user = new ApplicationUser()
            {
                Email = "TE.st@te.sst",
            };
            var topic = "TestTopic";
            var text = "TestText";
            var newForum = new Forum()
            {
                ForumTopic = topic,
                ForumText = text,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseInMemoryDatabase("forumTestLikePOST");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.Forums.AddAsync(newForum);
            await db.SaveChangesAsync();

            int expectedLikes = 1;

            var service = new ForumsService(db);

            await service.Like(newForum.ForumId, user.Id);
            var likedForum = await db.Forums.FirstOrDefaultAsync(f => f.ForumId == newForum.ForumId);

            Assert.NotNull(likedForum);
            Assert.Equal(expectedLikes, likedForum.Likes);
        }

        [Fact]
        public async Task LikeShouldThrowArgumentNullExceptionIfGivenInvalidForumId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
  .UseInMemoryDatabase("forumTestLikePOST");
            var db = new ApplicationDbContext(options.Options);

            var service = new ForumsService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.Like("fake_forum_id", "real_user_id"));
        }
    }
}
