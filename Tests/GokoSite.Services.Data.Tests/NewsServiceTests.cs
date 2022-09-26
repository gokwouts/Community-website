namespace GokoSite.Services.Data.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Data;
    using GokoSite.Data.Models;
    using GokoSite.Data.Models.News;
    using GokoSite.Web.ViewModels.News;
    using Microsoft.EntityFrameworkCore;

    using Xunit;

    public class NewsServiceTests
    {
        [Fact]
        public void GetNewShouldReturnNewDetailsPageViewModelIfNewWithGivenNewIdExists()
        {
            var testNewTitle = "Test";
            var testNewContent = "TestContentTestContentTestContent";

            var user = new ApplicationUser()
            {
                Email = "test.test@test.testing",
                PasswordHash = "4297F44B13955235245B2497399D7A93",
            };

            var newDb = new New()
            {
                Title = testNewTitle,
                Content = testNewContent,
                Image = "https://p.bigstockphoto.com/GeFvQkBbSLaMdpKXF1Zv_bigstock-Aerial-View-Of-Blue-Lakes-And--227291596.jpg",
                UserId = user.Id,
                UploadedOn = DateTime.UtcNow,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("test");
            var db = new ApplicationDbContext(options.Options);

            db.Users.Add(user);
            db.News.Add(newDb);
            db.SaveChanges();

            var service = new NewsService(db);

            var result = service.GetNew(newDb.NewId);
            Assert.NotNull(result);
            Assert.Equal(testNewTitle, result.Title);
            Assert.Equal(testNewContent, result.Content);
        }

        [Fact]
        public void GetNewShouldThrowArgumentExceptionIfGivenNotExistentNewId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("test");
            var db = new ApplicationDbContext(options.Options);

            var service = new NewsService(db);

            Assert.ThrowsAny<ArgumentNullException>(() => service.GetNew("fake id"));
        }

        [Fact]
        public async Task GetNewShouldThrowArgumentExceptionIfTheNewCreatorDoesNotExist()
        {
            var testNewTitle = "Test";
            var testNewContent = "TestContentTestContentTestContent";
            var testNewImage = "https://p.bigstockphoto.com/GeFvQkBbSLaMdpKXF1Zv_bigstock-Aerial-View-Of-Blue-Lakes-And--227291596.jpg";

            var newDbF = new New()
            {
                Title = testNewTitle,
                Content = testNewContent,
                Image = testNewImage,
                UserId = "fake_user_id",
                UploadedOn = DateTime.UtcNow,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("test");
            var db = new ApplicationDbContext(options.Options);

            await db.News.AddAsync(newDbF);
            await db.SaveChangesAsync();

            var service = new NewsService(db);

            Assert.ThrowsAny<ArgumentNullException>(() => service.GetNew(newDbF.NewId));
        }

        [Fact]
        public async Task GetNewsShouldReturnCollectionOfNewHomePageViewModelWithAllNewsInDb()
        {
            var user = new ApplicationUser()
            {
                Email = "test.test@test.testing",
                PasswordHash = "4297F44B13955235245B2497399D7A93",
            };

            var testNewTitle = "Test";
            var testNewContent = "TestContentTestContentTestContent";
            var testNewImage = "https://p.bigstockphoto.com/GeFvQkBbSLaMdpKXF1Zv_bigstock-Aerial-View-Of-Blue-Lakes-And--227291596.jpg";

            var newDbF = new New()
            {
                Title = testNewTitle,
                Content = testNewContent,
                Image = testNewImage,
                UserId = user.Id,
                UploadedOn = DateTime.UtcNow,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("testa");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.News.AddAsync(newDbF);
            await db.SaveChangesAsync();

            var service = new NewsService(db);

            var result = service.GetNews();
            var resultNew = result.First();

            Assert.NotNull(result);
            Assert.IsType<NewHomePageViewModel>(resultNew);
            Assert.Equal(newDbF.Image, resultNew.Image);
            Assert.Equal(newDbF.Title, resultNew.Title);
        }

        [Fact]
        public async Task RemoveNewShouldReturnTrueWhenSuccessfullyRemovedNew()
        {
            var user = new ApplicationUser()
            {
                Email = "test.test@test.testing",
                PasswordHash = "4297F44B13955235245B2497399D7A93",
            };

            var testNewTitle = "Test";
            var testNewContent = "TestContentTestContentTestContent";
            var testNewImage = "https://p.bigstockphoto.com/GeFvQkBbSLaMdpKXF1Zv_bigstock-Aerial-View-Of-Blue-Lakes-And--227291596.jpg";

            var newDbF = new New()
            {
                Title = testNewTitle,
                Content = testNewContent,
                Image = testNewImage,
                UserId = user.Id,
                UploadedOn = DateTime.UtcNow,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("test");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.News.AddAsync(newDbF);
            await db.SaveChangesAsync();

            var service = new NewsService(db);

            int currentDbNewsCount = db.News.Count();
            bool isDeleted = await service.RemoveNew(newDbF.NewId);

            Assert.True(isDeleted, "Remove New does not return true upon successfully removing a new!");
            Assert.Equal(currentDbNewsCount - 1, db.News.Count());
        }

        [Fact]
        public async Task RemoveNewShouldThrowArgumentNullExceptionIfGivenInvalidNewId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("test");
            var db = new ApplicationDbContext(options.Options);

            var service = new NewsService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.RemoveNew("fake_id"));
        }

        [Fact]
        public async Task EditNewShouldReturnTrueWhenSuccessfullyEditedNew()
        {
            var user = new ApplicationUser()
            {
                Email = "test.test@test.testing",
                PasswordHash = "4297F44B13955235245B2497399D7A93",
            };

            var testNewTitle = "Test";
            var testNewContent = "TestContentTestContentTestContent";
            var testNewImage = "https://p.bigstockphoto.com/GeFvQkBbSLaMdpKXF1Zv_bigstock-Aerial-View-Of-Blue-Lakes-And--227291596.jpg";

            var newDbF = new New()
            {
                Title = testNewTitle,
                Content = testNewContent,
                Image = testNewImage,
                UserId = user.Id,
                UploadedOn = DateTime.UtcNow,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("test");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.News.AddAsync(newDbF);
            await db.SaveChangesAsync();

            var service = new NewsService(db);

            var editedInput = new NewAddInputModel()
            {
                Title = "EditTitle",
                Content = "EditContent",
                Image = "EditedImage",
            };

            bool isEdited = await service.EditNew(editedInput, newDbF.NewId);
            var editedNew = db.News.First();

            Assert.True(isEdited, "Edit New does not return true upon successfully editing a new!");
            Assert.Equal(editedInput.Title, editedNew.Title);
            Assert.Equal(editedInput.Content, editedNew.Content);
            Assert.Equal(editedInput.Image, editedNew.Image);
        }

        [Fact]
        public async Task EditNewShouldThrowArgumentNullExceptionIfGivenInvalidNewId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("test");
            var db = new ApplicationDbContext(options.Options);

            var service = new NewsService(db);

            var editedInput = new NewAddInputModel()
            {
                Title = "EditTitle",
                Content = "EditContent",
                Image = "EditedImage",
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.EditNew(editedInput, "fake_id"));
        }

        [Fact]
        public async Task EditNewShouldThrowArgumentNullExceptionIfNotGivenEditInput()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("test");
            var db = new ApplicationDbContext(options.Options);

            var service = new NewsService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.EditNew(null, "realId"));
        }

        [Fact]
        public async Task AddNewShouldAddNewWithGivenValidNewAddInputModelAndUserId()
        {
            var user = new ApplicationUser()
            {
                Email = "test.test@test.testing",
                PasswordHash = "4297F44B13955235245B2497399D7A93",
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("testadd");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var service = new NewsService(db);

            var testNewTitle = "Test";
            var testNewContent = "TestContentTestContentTestContent";
            var testNewImage = "https://p.bigstockphoto.com/GeFvQkBbSLaMdpKXF1Zv_bigstock-Aerial-View-Of-Blue-Lakes-And--227291596.jpg";

            var addNewInput = new NewAddInputModel()
            {
                Title = testNewTitle,
                Content = testNewContent,
                Image = testNewImage,
            };

            int newsCountBeforeAdd = db.News.Count();

            await service.AddNew(addNewInput, user.Id);

            var addedNew = db.News.First();

            Assert.Equal(newsCountBeforeAdd + 1, db.News.Count());
            Assert.Equal(addNewInput.Title, addedNew.Title);
            Assert.Equal(addNewInput.Content, addedNew.Content);
            Assert.Equal(addNewInput.Image, addedNew.Image);
        }

        [Fact]
        public async Task AddNewShouldThrowArgumentNullExceptionIfNotGivenAddInput()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("test");
            var db = new ApplicationDbContext(options.Options);

            var service = new NewsService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddNew(null, "userId"));
        }

        [Fact]
        public async Task AddNewShouldThrowArgumentNullExceptionIfGivenInvalidUserId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("test");
            var db = new ApplicationDbContext(options.Options);

            var service = new NewsService(db);

            var testNewTitle = "Test";
            var testNewContent = "TestContentTestContentTestContent";
            var testNewImage = "https://p.bigstockphoto.com/GeFvQkBbSLaMdpKXF1Zv_bigstock-Aerial-View-Of-Blue-Lakes-And--227291596.jpg";

            var addNewInput = new NewAddInputModel()
            {
                Title = testNewTitle,
                Content = testNewContent,
                Image = testNewImage,
            };

            await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddNew(addNewInput, "fake_user_id"));
        }
    }
}
