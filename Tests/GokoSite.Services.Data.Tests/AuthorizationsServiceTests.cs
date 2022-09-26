namespace GokoSite.Services.Data.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Data;
    using GokoSite.Data.Models;
    using Microsoft.EntityFrameworkCore;

    using Xunit;

    public class AuthorizationsServiceTests
    {
        [Fact]
        public async Task AddAdministratorShouldAddUserWithGivenEmailAsAdministrator()
        {
            var userEmail = "t.est@tes.t";
            var roleName = "Administrator";

            var user = new ApplicationUser()
            {
                Email = userEmail,
            };

            var role = new ApplicationRole()
            {
                Name = roleName,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("authtestr");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.Roles.AddAsync(role);
            await db.SaveChangesAsync();

            var service = new AuthorizationsService(db);

            await service.AddAdministrator(userEmail);

            var userRole = db.UserRoles.FirstOrDefault(ur => ur.UserId == user.Id && ur.RoleId == role.Id);

            Assert.NotNull(userRole);
        }

        [Fact]
        public async Task AddAdministratorShouldThrowArgumentNullExceptionIfGivenNoEmail()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("authtest");
            var db = new ApplicationDbContext(options.Options);

            var service = new AuthorizationsService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.AddAdministrator(null));
        }

        [Fact]
        public async Task AddAdministratorShouldThrowArgumentNullExceptionIfGivenInvalidEmail()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("authtest");
            var db = new ApplicationDbContext(options.Options);

            var service = new AuthorizationsService(db);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.AddAdministrator("fake_Email@a.b"));
        }

        [Fact]
        public async Task AddAdministratorShouldThrowInvalidOperationExceptionIfThereIsNoAdministrationRole()
        {
            var userEmail = "t.est@tes.t";

            var user = new ApplicationUser()
            {
                Email = userEmail,
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("authtest");
            var db = new ApplicationDbContext(options.Options);

            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            var service = new AuthorizationsService(db);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.AddAdministrator(user.Email));
        }
    }
}
