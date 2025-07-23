using Microsoft.Playwright;

namespace ELTE.TravelAgency.Admin.Blazor.E2ETest
{
    public class AuthTest : TestBase
    {
        [Fact]
        public async Task RedirectToLoginTest()
        {
            // Navigate to home page
            await Page.GotoAsync("/");

            // Wait for redirection to the login page
            await Page.WaitForURLAsync("**/login");
        }

        [Fact]
        public async Task ValidLoginTest()
        {
            // Navigate to login page
            await Page.GotoAsync("/login");

            // Find input fields by their IDs, then fill in credentials and click login button
            var emailInput = Page.Locator("#email");
            var passwordInput = Page.Locator("#password");
            var loginButton = Page.GetByRole(AriaRole.Button, new() { Name = "Bejelentkezés" });

            await emailInput.FillAsync("admin@example.com");
            await passwordInput.FillAsync("Admin@123");
            await loginButton.ClickAsync();

            // Expect logout button to exists and be visible
            var logoutButton = Page.GetByRole(AriaRole.Button, new() { Name = "Kijelentkezés" });
            await Expect(logoutButton).ToBeVisibleAsync();
        }

        [Fact]
        public async Task InvalidLoginTest()
        {
            await Page.GotoAsync("/login");

            var emailInput = Page.Locator("#email");
            var passwordInput = Page.Locator("#password");
            var loginButton = Page.GetByRole(AriaRole.Button, new() { Name = "Bejelentkezés" });

            await emailInput.FillAsync("admin@example.com");
            await passwordInput.FillAsync("Bad@987");
            await loginButton.ClickAsync();

            // Expect logout button to not exist
            var logoutButton = Page.GetByRole(AriaRole.Button, new() { Name = "Kijelentkezés" });
            await Expect(logoutButton).ToHaveCountAsync(0);
        }
    }

}