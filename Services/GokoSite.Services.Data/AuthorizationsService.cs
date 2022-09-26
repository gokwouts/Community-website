namespace GokoSite.Services.Data
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using GokoSite.Data;

    public class AuthorizationsService : IAuthorizationsService
    {
        private readonly ApplicationDbContext db;

        public AuthorizationsService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task AddAdministrator(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException("email", "The given email is invalid!");
            }

            var user = this.db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                throw new ArgumentNullException("email", "There is no user with the given email!");
            }

            var administratorRole = this.db.Roles.FirstOrDefault(r => r.Name == "Administrator");

            if (administratorRole == null)
            {
                throw new InvalidOperationException($"There is no role with the name \"{"Administrator"}\"!");
            }

            this.db.UserRoles.Add(new Microsoft.AspNetCore.Identity.IdentityUserRole<string>()
            {
                RoleId = administratorRole.Id,
                UserId = user.Id,
            });

            await this.db.SaveChangesAsync();
        }
    }
}
