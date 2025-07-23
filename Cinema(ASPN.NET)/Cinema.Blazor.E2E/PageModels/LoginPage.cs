using Microsoft.Playwright;

namespace ELTE.Cinema.Blazor.E2E.PageModels;

public class LoginPage
{
    private readonly IPage _page;

    public LoginPage(IPage page)
    {
        _page = page;
    }

    public async Task LoginAsync(string username, string password)
    {
        // Navigate to login page
        await _page.GotoAsync("/login");
        
        // Find input fields by their IDs
        var emailInput = _page.Locator("#email");
        var passwordInput = _page.Locator("#password");
        var loginButton = _page.GetByRole(AriaRole.Button, new() { Name = "Login" });

        // Fill in credentials and click login button
        await emailInput.FillAsync(username);
        await passwordInput.FillAsync(password);
        await loginButton.ClickAsync();
    }
}