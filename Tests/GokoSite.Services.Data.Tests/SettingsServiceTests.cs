namespace GokoSite.Services.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Data;
    using GokoSite.Data.Common.Repositories;
    using GokoSite.Data.Models;
    using GokoSite.Data.Repositories;

    using Microsoft.EntityFrameworkCore;

    using Moq;

    using Xunit;

    public class SettingsServiceTests
    {
        [Fact]
        public void GetCountShouldReturnCorrectNumber()
        {
            var repository = new Mock<IDeletableEntityRepository<Setting>>();
            repository.Setup(r => r.All()).Returns(new List<Setting>
                                                        {
                new Setting()
                                                        }.AsQueryable());
            var service = new SettingsService(repository.Object);
            Assert.Equal(0, service.GetCount());
        }

        [Fact]
        public async Task GetCountShouldReturnCorrectNumberUsingDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SettingsTestDbА").Options;
            using var dbContext = new ApplicationDbContext(options);
            await dbContext.Settings.AddAsync(new Setting());
            await dbContext.Settings.AddAsync(new Setting());
            await dbContext.Settings.AddAsync(new Setting());
            await dbContext.SaveChangesAsync();

            using var repository = new EfDeletableEntityRepository<Setting>(dbContext);
            var service = new SettingsService(repository);
            Assert.Equal(3, service.GetCount());
        }

        [Fact]
        public async Task GetAllShouldReturnAllSettings()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "SettingsTestDb").Options;
            using var dbContext = new ApplicationDbContext(options);
            await dbContext.Settings.AddAsync(new Setting());
            await dbContext.SaveChangesAsync();
            using var repository = new EfDeletableEntityRepository<Setting>(dbContext);
            var service = new SettingsService(repository);
            Assert.Throws<NullReferenceException>(() => service.GetAll<Setting>());
        }
    }
}
