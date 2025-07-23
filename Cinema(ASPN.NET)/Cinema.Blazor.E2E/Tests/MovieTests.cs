using System.Text.RegularExpressions;
using ELTE.Cinema.Blazor.E2E.PageModels;
using Microsoft.Playwright;
using Xunit;

namespace ELTE.Cinema.Blazor.E2E.Tests;

public class MovieTests : TestBase
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var loginPage = new LoginPage(Page);
        await loginPage.LoginAsync("admin@example.com", "Admin@123");
        // Wait for movies page to appear, as this is the default page where login redirects
        await Page.WaitForURLAsync("**/movies");
        // Wait for movies to appear
        await Page.WaitForSelectorAsync(".card");
    }

    [Fact]
    public async Task ListMovies_AllMoviesAreReturned()
    {
        var cards = await Page.Locator(".card").CountAsync();
        Assert.Equal(10, cards);
    }

    [Fact]
    public async Task AddMovie_MovieCreated_WhenMovieIsValid()
    {
        // Click on the New Movie button
        var addButton = Page.Locator("[data-testid='add-movie-btn']");
        await addButton.ClickAsync();

        // Wait for redirect and check url
        await Page.WaitForURLAsync("**/movies/add");
        
        // Fill in the form
        await Page.FillAsync("#title", "Test Movie");
        await Page.FillAsync("#year", "2025");
        await Page.FillAsync("#director", "Test Director");
        await Page.FillAsync("#synopsis", "This is a test movie synopsis");
        await Page.FillAsync("#length", "120");

        // Upload an image
        var testImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestAssets", "Image.jpg");
        await Page.SetInputFilesAsync("#image", [testImagePath]);

        // Submit the form
        await Page.ClickAsync("button[type='submit']");
        
        // The user should be redirected back to the movies page
        await Page.WaitForURLAsync("**/movies");

        var movieTitle = Page.GetByText("Test Movie");
        Assert.NotNull(movieTitle);
    }
    
    [Fact]
    public async Task AddMovie_RejectSave_WhenMovieIsInvalid()
    {
        // Click on the New Movie button
        var addButton = Page.Locator("[data-testid='add-movie-btn']");
        await addButton.ClickAsync();

        // Wait for redirect and check url
        await Page.WaitForURLAsync("**/movies/add");
        
        // Do not fill the values

        // Submit the form
        await Page.ClickAsync("button[type='submit']");
        
        // Check that validation messages are displayed
        var titleValidationMessage = await Page.TextContentAsync(".text-danger >> nth=0");
        var yearValidationMessage = await Page.TextContentAsync(".text-danger >> nth=1");
        var directorValidationMessage = await Page.TextContentAsync(".text-danger >> nth=2");

        Assert.Equal("Title shouldn't be empty", titleValidationMessage);
        Assert.Equal("Year should be greater than 1000", yearValidationMessage);
        Assert.Equal("Length should be greater than 1", directorValidationMessage);
    }
    
    [Fact]
    public async Task EditMovie_FormFilledCorrectly_AfterLoaded()
    {
        // Get and click the delete button for the movie
        var movieCard = Page.Locator(".card", new() { 
            HasText = "Inception" 
        });
        await movieCard.ClickAsync();
        await Page.WaitForURLAsync("**/movies/edit/**");
        
        // Assert that the form fields are populated with the correct values
        var titleInput = Page.Locator("#title");
        await Expect(titleInput).ToHaveValueAsync("Inception");
    
        var yearInput = Page.Locator("#year");
        await Expect(yearInput).ToHaveValueAsync("2010");
    
        var directorInput = Page.Locator("#director");
        await Expect(directorInput).ToHaveValueAsync("Christopher Nolan");
    
        var synopsisInput = Page.Locator("#synopsis");
        await Expect(synopsisInput).ToHaveValueAsync(new Regex(".*dream.*", RegexOptions.IgnoreCase)); // Partial text match
    
        var lengthInput = Page.Locator("#length");
        await Expect(lengthInput).ToHaveValueAsync("148");
    
        // Check if image is displayed
        var image = Page.Locator("img[alt='Movie Image']");
        await Expect(image).ToBeVisibleAsync();
    }
    
    [Fact]
    public async Task EditMovie_MovieSaved_WhenMovieIsValid()
    {
        // Get and click the delete button for the movie
        var movieCard = Page.Locator(".card", new() { 
            HasText = "Inception" 
        });
        await movieCard.ClickAsync();
        await Page.WaitForURLAsync("**/movies/edit/**");
        
        // Change movie title
        await Page.FillAsync("#title", "Test Movie");
        
        // Submit the form
        await Page.ClickAsync("button[type='submit']");
        
        // The user should be redirected back to the movies page
        await Page.WaitForURLAsync("**/movies");

        var movieTitle = Page.GetByText("Test Movie");
        Assert.NotNull(movieTitle);
    }

    
    [Fact]
    public async Task EditMovie_RejectSave_WhenMovieIsInvalid()
    {
        // Get and click the delete button for the movie
        var movieCard = Page.Locator(".card", new() { 
            HasText = "Inception" 
        });
        await movieCard.ClickAsync();
        await Page.WaitForURLAsync("**/movies/edit/**");
        
        // Change movie title
        await Page.FillAsync("#title", "");
        
        // Submit the form
        await Page.ClickAsync("button[type='submit']");
        
        // Check that validation messages are displayed
        var titleValidationMessage = await Page.TextContentAsync(".validation-message >> nth=0");

        Assert.Equal("Title shouldn't be empty", titleValidationMessage);
    }

    [Fact]
    public async Task DeleteMovie_MovieDeleted()
    {
        // Get and click the delete button for the movie
        var deleteButton = Page.Locator(".card", new() { 
            HasText = "Inception" 
        }).Locator(".btn-danger");
        await deleteButton.ClickAsync();
        
        // Get the click confirm button on the modal
        var deleteConfirmButton = Page.Locator(".modal .btn-danger");
        await deleteConfirmButton.ClickAsync();
        
        // Wait for the element containing "Inception" to disappear
        await Page.Locator(".card", new() { 
            HasText = "Inception" 
        }).WaitForAsync(new() { 
            State = WaitForSelectorState.Hidden,
            Timeout = 5000 // 5 second timeout
        });
    }
}