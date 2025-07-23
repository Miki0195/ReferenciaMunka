using ELTE.Cinema.Blazor.E2E.PageModels;
using Xunit;

namespace ELTE.Cinema.Blazor.E2E.Tests;

public class AuthTests : TestBase
{
    [Fact]
    public async Task TestRedirectToLoginPage()
    {
        // Navigate to home page
        await Page.GotoAsync("/movies");
        
        // Wait for redirection to the login page
        await Page.WaitForURLAsync("**/login");
    }
    
    [Fact]
    public async Task Login_UserRedirected_WhenFormIsValid()
    {
        // Navigate to home page
        // await Page.GotoAsync("/login");

        // Find input fields by their IDs, then fill in credentials and click login button.
        // var emailInput = Page.Locator("#email");
        // var passwordInput = Page.Locator("#password");
        // var loginButton = Page.GetByRole(AriaRole.Button, new() { Name = "Login" });
        //
        // await emailInput.FillAsync("admin@example.com");
        // await passwordInput.FillAsync("Admin@123");
        // await loginButton.ClickAsync();

        // We can move the login test logic to a fixture instead for reusability
        var loginPage = new LoginPage(Page);
        await loginPage.LoginAsync("admin@example.com", "Admin@123");
        
        // Check if user redirected correctly
        await Page.WaitForURLAsync("**/movies");
    }
    
    [Fact]
    public async Task Login_LoginRejected_WhenFormisInvalid()
    {
        // Login with invalid credentials
        var loginPage = new LoginPage(Page);
        await loginPage.LoginAsync("admin@example.com", "Admin@1234");
    
        // Wait for redirection back to the login page
        await Page.WaitForURLAsync("**/login");
    
        // Check for error message or indication of login failure
        var errorMessage = Page.Locator(".toast");
        await Expect(errorMessage).ToBeVisibleAsync();
    
        // Optionally, check the specific error message text
        var errorText = await errorMessage.TextContentAsync();
        Assert.Contains("Email or password is invalid", errorText, StringComparison.OrdinalIgnoreCase);
    
        // Ensure the user remains on the login page
        Assert.Contains("/login", Page.Url);
    }
    
    [Fact]
    public async Task Logout_UserRedirected()
    {
        // Login
        var loginPage = new LoginPage(Page);
        await loginPage.LoginAsync("admin@example.com", "Admin@123");
        
        // Wait for redirection to the movies page
        await Page.WaitForURLAsync("**/movies");
        
        // Find logout button and call logout
        var logoutButton = Page.Locator(".navbar .container button");
        await logoutButton.ClickAsync();
        
        // Wait for redirection to the login page 
        await Page.WaitForURLAsync("**/login");
    }
}