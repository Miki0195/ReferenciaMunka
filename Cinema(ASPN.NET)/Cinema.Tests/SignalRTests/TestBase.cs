using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using ELTE.Cinema.DataAccess;
using ELTE.Cinema.DataAccess.Models;
using ELTE.Cinema.DataAccess.Services;
using ELTE.Cinema.Shared.Models;
using ELTE.Cinema.WebAPI;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ELTE.Cinema.Tests.SignalRTests
{
    public abstract class TestBase : IDisposable
    {
        protected string HubName = string.Empty;
        protected readonly WebApplicationFactory<Program> Factory;
        protected readonly HttpClient Client;

        protected readonly UserRequestDto AdminUser = new()
        {
            Email = "admin@example.com",
            Name = "Test Admin",
            Password = "testAdmin123!",
            PhoneNumber = "06301111111"
        };

        protected readonly UserRequestDto User = new()
        {
            Email = "user@example.com",
            Name = "Test User",
            Password = "testUser123!",
            PhoneNumber = "06301111111"
        };

        protected TestBase(string dbName)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "IntegrationTest");

            Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace the real database with an in-memory database
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<CinemaDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<CinemaDbContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName)
                            .UseLazyLoadingProxies();
                    });

                    //Seed the database with initial data
                    using var scope = services.BuildServiceProvider().CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<CinemaDbContext>();
                    db.Database.EnsureCreated();

                    SeedDatabase(db);

                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserRole>>();
                    SeedRoles(roleManager);

                    var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                    var userService = scope.ServiceProvider.GetRequiredService<IUsersService>();
                    userService.AddUserAsync(mapper.Map<User>(User), User.Password).Wait();
                    userService.AddUserAsync(mapper.Map<User>(AdminUser), AdminUser.Password, Role.Admin).Wait();

                });
            });

            Client = Factory.CreateClient();
        }

        protected async Task<string> Login(UserRequestDto user)
        {
            var loginRequestDto = new LoginRequestDto
            {
                Email = user.Email,
                Password = user.Password
            };

            var response = await Client.PostAsJsonAsync("users/login", loginRequestDto);
            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

            if (loginResponse?.AuthToken == null)
                throw new Exception("Login failed");

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.AuthToken);
            
            return loginResponse.AuthToken;
        }

        protected HubConnection CreateHubConnection(string? token = null)
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl($"http://localhost/{HubName}", options =>
                {
                    options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                    
                    if (token != null)
                    {
                        options.AccessTokenProvider = () => Task.FromResult(token)!;
                    }
                })
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .AddJsonProtocol()
                .Build();

            return hubConnection;
        }

        private static void SeedDatabase(CinemaDbContext context)
        {
            var screening = new Screening
            {
                Movie = new Movie { Id = 1, Director = "Test Director", Length = 120, Year = 2024, Title = "Test Movie", CreatedAt = DateTime.UtcNow, Image = [], Synopsis = "" },
                Room = new Room { Id = 1, Rows = 10, Columns = 10, Name = "Room 1", CreatedAt = DateTime.UtcNow },
                Seats = new List<Seat>(),
                CreatedAt = DateTime.UtcNow,
                StartsAt = DateTime.UtcNow.AddDays(1)
            };

            context.Screenings.Add(screening);

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
        public async void Dispose()
        {
            using var scope = Factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CinemaDbContext>();
            await db.Database.EnsureDeletedAsync();

            Client.Dispose();
            await Factory.DisposeAsync();
        }
    }
}
