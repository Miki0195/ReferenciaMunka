using Microsoft.Playwright;

namespace ELTE.TravelAgency.Admin.Blazor.E2ETest
{
    public class BuildingsTest : TestBase
    {
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            // Do valid login as admin
            await Page.GotoAsync("/login");

            var emailInput = Page.Locator("#email");
            var passwordInput = Page.Locator("#password");
            var loginButton = Page.GetByRole(AriaRole.Button, new() { Name = "Bejelentkezés" });

            await emailInput.FillAsync("admin@example.com");
            await passwordInput.FillAsync("Admin@123");
            await loginButton.ClickAsync();

            var logoutButton = Page.GetByRole(AriaRole.Button, new() { Name = "Kijelentkezés" });
            await Expect(logoutButton).ToBeVisibleAsync();
        }


        [Fact]
        public async Task ListBuildingsTest()
        {
            var loadBuildingsButton = Page.GetByRole(AriaRole.Button, new() { Name = "Épületek lekérdezése" });
            await Expect(loadBuildingsButton).ToBeVisibleAsync();

            await loadBuildingsButton.ClickAsync();

            await Page.Locator("table").WaitForAsync();
            var tableRows = await Page.Locator("table tr").CountAsync();
            Assert.Equal(5, tableRows);
        }

    }
}