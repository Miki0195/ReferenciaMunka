using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ELTE.Cinema.DataAccess;
using ELTE.Cinema.DataAccess.Models;
using ELTE.Cinema.Shared.Models;
using ELTE.Cinema.WebAPI;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ELTE.Cinema.Tests.IntegrationTests;

public class MoviesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;
    
    private static readonly LoginRequestDto AdminLogin = new()
    {
        Email = "admin@example.com",
        Password = "Admin@123"
    };
    
    private static readonly LoginRequestDto UserLogin = new()
    {
        Email = "user@example.com",
        Password = "User@123"
    };
    
    public MoviesControllerIntegrationTests()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "IntegrationTest");
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace the real database with an in-memory database
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<CinemaDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<CinemaDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestMoviesDatabase");
                });

                //Seed the database with initial data
                using var scope = services.BuildServiceProvider().CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<CinemaDbContext>();
                db.Database.EnsureCreated();

                SeedMovies(db);

                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserRole>>();
                SeedRoles(roleManager);

                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                SeedUsers(userManager);
            });
        });

        _client = _factory.CreateClient();
    }

    #region Get

    [Fact]
    public async Task GetMovies_ReturnsAllMovies_WithoutDeleted()
    {
        // Act
        var response = await _client.GetAsync("/movies");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var movies = await response.Content.ReadFromJsonAsync<List<MovieResponseDto>>();
        Assert.NotNull(movies);
        Assert.True(movies.Count == 2); //Third movie is deleted, should not be returned
    }

    [Fact]
    public async Task GetMovieById_ReturnsMovie_WhenMovieExists()
    {
        // Arrange
        const int movieId = 1;

        // Act
        var response = await _client.GetAsync($"/movies/{movieId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var movie = await response.Content.ReadFromJsonAsync<MovieResponseDto>();
        Assert.NotNull(movie);
        Assert.Equal(movieId, movie.Id);
        Assert.Equal("Test Movie 1", movie.Title);
    }

    [Fact]
    public async Task GetMovieById_ReturnsNotFound_WhenMovieNotExists()
    {
        // Arrange
        var movieId = 99; // ID of a seeded movie

        // Act
        var response = await _client.GetAsync($"/movies/{movieId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task GetMovieById_ReturnsNotFound_WhenMovieDeleted()
    {
        // Arrange
        var movieId = 3; // ID of a seeded movie

        // Act
        var response = await _client.GetAsync($"/movies/{movieId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
    }

    #endregion

    #region Create

    [Fact]
    public async Task CreateMovie_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        // Arrange
        var newMovie = new MovieRequestDto
        {
            Title = "New Test Movie",
            Year = 2022,
            Director = "New Director",
            Synopsis = "New Synopsis",
            Length = 100,
            Image = new byte[] { 4, 5, 6 }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/movies", newMovie);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task CreateMovie_ReturnsForbidden_WhenNotAdmin()
    {
        // Arrange
        var newMovie = new MovieRequestDto
        {
            Title = "New Test Movie",
            Year = 2022,
            Director = "New Director",
            Synopsis = "New Synopsis",
            Length = 100,
            Image = new byte[] { 4, 5, 6 }
        };

        // Act
        await Login(UserLogin);
        var response = await _client.PostAsJsonAsync("/movies", newMovie);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
    }

    [Fact]
    public async Task CreateMovie_ReturnsBadRequest_WhenDataIsInvalid()
    {
        // Arrange
        var invalidMovie = new MovieRequestDto
        {
            Title = "", // Invalid title
            Year = 900, // Invalid year
            Director = new string('D', 300), //Invalid director length
            Synopsis = "Synopsis",
            Length = 0, // Invalid length
            Image = new byte[] { 1, 2, 3 }
        };

        // Act
        await Login(AdminLogin);
        var response = await _client.PostAsJsonAsync("/movies", invalidMovie);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Contains("Title", problemDetails.Errors.Keys);
        Assert.Contains("Year", problemDetails.Errors.Keys);
        Assert.Contains("Director", problemDetails.Errors.Keys);
        Assert.Contains("Length", problemDetails.Errors.Keys);
    }

    [Fact]
    public async Task CreateMovie_ReturnsConflict_WhenTitleExists()
    {
        // Arrange
        var newMovie = new MovieRequestDto
        {
            Title = "Test Movie 1",
            Year = 2022,
            Director = "New Director",
            Synopsis = "New Synopsis",
            Length = 100,
            Image = new byte[] { 4, 5, 6 }
        };

        // Act
        await Login(AdminLogin);
        var response = await _client.PostAsJsonAsync("/movies", newMovie);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
    }

    [Fact]
    public async Task CreateMovie_AddsMovie_WhenDataIsValid()
    {
        // Arrange
        var newMovie = new MovieRequestDto
        {
            Title = "New Test Movie",
            Year = 2022,
            Director = "New Director",
            Synopsis = "New Synopsis",
            Length = 100,
            Image = new byte[] { 4, 5, 6 }
        };


        // Act
        await Login(AdminLogin);
        var response = await _client.PostAsJsonAsync("/movies", newMovie);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdMovie = await response.Content.ReadFromJsonAsync<MovieResponseDto>();
        Assert.NotNull(createdMovie);
        Assert.Equal(newMovie.Title, createdMovie.Title);
        Assert.Equal(newMovie.Director, createdMovie.Director);
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateMovie_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        // Arrange
        const int movieId = 1;
        var updatedMovie = new MovieRequestDto
        {
            Title = "Updated Movie Title",
            Year = 2023,
            Director = "Updated Director",
            Synopsis = "Updated Synopsis",
            Length = 130,
            Image = [7, 8, 9]
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/movies/{movieId}", updatedMovie);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task UpdateMovie_ReturnsForbidden_WhenNotAdmin()
    {
        // Arrange
        const int movieId = 1;
        var updatedMovie = new MovieRequestDto
        {
            Title = "Updated Movie Title",
            Year = 2023,
            Director = "Updated Director",
            Synopsis = "Updated Synopsis",
            Length = 130,
            Image = [7, 8, 9]
        };

        // Act
        await Login(UserLogin);
        var response = await _client.PutAsJsonAsync($"/movies/{movieId}", updatedMovie);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
    }

    [Fact]
    public async Task UpdateMovie_ReturnsNotFound_WhenMovieNotExists()
    {
        // Arrange
        const int movieId = 99;
        var updatedMovie = new MovieRequestDto
        {
            Title = "Updated Movie Title",
            Year = 2023,
            Director = "Updated Director",
            Synopsis = "Updated Synopsis",
            Length = 130,
            Image = [7, 8, 9]
        };

        // Act
        await Login(AdminLogin);
        var response = await _client.PutAsJsonAsync($"/movies/{movieId}", updatedMovie);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
    }

    [Fact]
    public async Task UpdateMovie_ReturnsBadRequest_WhenDataIsInValid()
    {
        // Arrange
        const int movieId = 1;
        var updatedMovie = new MovieRequestDto
        {
            Title = "", // Invalid title
            Year = 900, // Invalid year
            Director = new string('D', 300), //Invalid director length
            Synopsis = "Synopsis",
            Length = 0, // Invalid length
            Image = new byte[] { 1, 2, 3 }
        };

        // Act
        await Login(AdminLogin);
        var response = await _client.PutAsJsonAsync($"/movies/{movieId}", updatedMovie);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Contains("Title", problemDetails.Errors.Keys);
        Assert.Contains("Year", problemDetails.Errors.Keys);
        Assert.Contains("Director", problemDetails.Errors.Keys);
        Assert.Contains("Length", problemDetails.Errors.Keys);
    }

    [Fact]
    public async Task UpdateMovie_ReturnsConflict_WhenTitleExists()
    {
        // Arrange
        const int movieId = 1;
        var updatedMovie = new MovieRequestDto
        {
            Title = "Test Movie 2",
            Year = 2023,
            Director = "Updated Director",
            Synopsis = "Updated Synopsis",
            Length = 130,
            Image = [7, 8, 9]
        };

        // Act
        await Login(AdminLogin);
        var response = await _client.PutAsJsonAsync($"/movies/{movieId}", updatedMovie);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
    }

    [Fact]
    public async Task UpdateMovie_UpdatesExistingMovie_WhenDataIsValid()
    {
        // Arrange
        const int movieId = 1;
        var updatedMovie = new MovieRequestDto
        {
            Title = "Updated Movie Title",
            Year = 2023,
            Director = "Updated Director",
            Synopsis = "Updated Synopsis",
            Length = 130,
            Image = [7, 8, 9]
        };

        // Act
        await Login(AdminLogin);
        var response = await _client.PutAsJsonAsync($"/movies/{movieId}", updatedMovie);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedResponseMovie = await response.Content.ReadFromJsonAsync<MovieResponseDto>();
        Assert.NotNull(updatedResponseMovie);
        Assert.Equal(updatedMovie.Title, updatedResponseMovie.Title);
        Assert.Equal(updatedMovie.Director, updatedResponseMovie.Director);
    }


    #endregion

    #region Delete

    [Fact]
    public async Task DeleteMovie_ReturnsUnauthorized_WhenNotLoggedIn()
    {
        // Arrange
        const int movieId = 1;

        // Act
        var response = await _client.DeleteAsync($"/movies/{movieId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
    }
    
    [Fact]
    public async Task DeleteMovie_ReturnsForbidden_WhenNotAdmin()
    {
        // Arrange
        const int movieId = 1;

        // Act
        await Login(UserLogin);
        var response = await _client.DeleteAsync($"/movies/{movieId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
    }

    [Fact]
    public async Task DeleteMovie_ReturnsNotFound_WhenMovieNotExists()
    {
        // Arrange
        const int movieId = 99;

        // Act
        await Login(AdminLogin);
        var response = await _client.DeleteAsync($"/movies/{movieId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problemDetails);
    }

    [Fact]
    public async Task DeleteMovie_RemovesMovie_WhenMovieExists()
    {
        // Arrange
        const int movieId = 1;

        // Act
        await Login(AdminLogin);
        var response = await _client.DeleteAsync($"/movies/{movieId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify movie was deleted
        var getResponse = await _client.GetAsync($"/movies/{movieId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    #endregion

    #region Helpers
    private void SeedMovies(CinemaDbContext context)
    {
        context.Movies.AddRange(
            new Movie { Title = "Test Movie 1", Year = 2020, Director = "Director 1", Synopsis = "Synopsis 1", Length = 120, Image = new byte[] { 1, 2, 3 } },
            new Movie { Title = "Test Movie 2", Year = 2021, Director = "Director 2", Synopsis = "Synopsis 2", Length = 90, Image = new byte[] { 1, 2, 3 } },
            new Movie { Title = "Test Movie 3", Year = 2021, Director = "Director 3", Synopsis = "Synopsis 3", Length = 90, Image = new byte[] { 1, 2, 3 }, DeletedAt = DateTime.UtcNow}
        );

        context.SaveChanges();
    }

    private static void SeedRoles(RoleManager<UserRole> roleManager)
    {
        string[] roleNames = ["Admin"];

        foreach (var roleName in roleNames)
        {
            var roleExist = roleManager.RoleExistsAsync(roleName).Result;
            if (!roleExist)
            {
                // Create the roles and seed them to the database
                roleManager.CreateAsync(new UserRole(roleName)).Wait();
            }
        }
    }

    private static void SeedUsers(UserManager<User> userManager)
    {
        // Example to seed an Admin user
        var adminUser = userManager.FindByEmailAsync(AdminLogin.Email).Result;
        if (adminUser == null)
        {
            adminUser = new User { UserName = AdminLogin.Email, Email = AdminLogin.Email, Name = "Test Admin" };
            userManager.CreateAsync(adminUser, AdminLogin.Password).Wait();
            userManager.AddToRoleAsync(adminUser, "Admin").Wait();
        }
        
        // Example to seed normal user
        var user = userManager.FindByEmailAsync(UserLogin.Email).Result;
        if (user == null)
        {
            user = new User { UserName = UserLogin.Email, Email = UserLogin.Email, Name = "Test User" };
            userManager.CreateAsync(user, UserLogin.Password).Wait();
        }
    }

    private async Task Login(LoginRequestDto loginRequest)
    {
        var response = await _client.PostAsJsonAsync("users/login", loginRequest);
        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse?.AuthToken);
    }

    public void Dispose()
    {
        using var scope = _factory.Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<CinemaDbContext>();
        db.Database.EnsureDeleted();

        _factory.Dispose();
        _client.Dispose();
    }

    #endregion
}