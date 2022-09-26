namespace GokoSite.WebUI.Tests
{
    using GokoSite.Data;
    using GokoSite.Services.Data;
    using GokoSite.Web.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class AdministrationControllerTests
    {
        [Fact]
        public void TryingToAccesAdminPanelWithoutPermissionShouldRedirectToAccesDeniedPage()
        {

        }
    }
}
