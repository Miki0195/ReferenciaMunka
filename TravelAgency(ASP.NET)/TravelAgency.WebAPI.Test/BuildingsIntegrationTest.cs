using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using ELTE.TravelAgency.Admin.Persistence;
using ELTE.TravelAgency.DataAccess;
using ELTE.TravelAgency.DataAccess.Models;
using ELTE.TravelAgency.Shared.Models;
using ELTE.TravelAgency.WebAPI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AutoMapper;
using Xunit;

namespace ELTE.TravelAgency.WebAPI.Test
{
	public class BuildingsIntegrationTest : IDisposable
	{
	    public static IList<City> CityData = new List<City>
	    {
	        new City { Id = 1, Name = "TESTCITY" }
	    };

	    public static IList<Building> BuildingData = new List<Building>
	    {
			new Building
            {
                Id = 1,
                CityId = CityData[0].Id,
                City = CityData[0],
                Name = "TESTBUILDING1",
                SeaDistance = 1,
                Shore = ShoreType.Rocky,
                Features = Feature.CoastService | Feature.MainRoad,
                Comment = "Test Building 1"
            },
            new Building
            {
                Id = 2,
                CityId = CityData[0].Id,
                City = CityData[0],
                Name = "TESTBUILDING2",
                SeaDistance = 10,
                Shore = ShoreType.Gravelly,
                Features = Feature.None,
                Comment = "Test Building 2"
            },
            new Building
            {
                Id = 3,
                CityId = CityData[0].Id,
                City = CityData[0],
                Name = "TESTBUILDING3",
                SeaDistance = 100,
                Shore = ShoreType.Sandy,
                Features = Feature.PrivateParking,
                Comment = "Test Building 3"
            }
		};

        private readonly WebApplicationFactory<Program> _server;
        private readonly HttpClient _client;
        private readonly ITravelAgencyPersistence _persistence;
        private readonly List<BuildingDto> _buildingDtos;
        private readonly List<CityDto> _cityDtos;

        public BuildingsIntegrationTest()
        {
            // függőségek inicializációja
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
            var mapper = mapperConfig.CreateMapper();

            // DTO-k mappelése későbbi ellenőrzésekhez
            _buildingDtos = mapper.Map<List<BuildingDto>>(BuildingData);
            _cityDtos = mapper.Map<List<CityDto>>(CityData);

            // szerver konfiguráció összeállítása és elindítása
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
            _server = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    // teszt szerver konfiguráció eltérései
                    builder.ConfigureTestServices(async (services) =>
                    {
                        // Program osztályban megadott adatbázis kontextus eltávolítása
                        services.RemoveAll(typeof(DbContextOptions<TravelAgencyContext>));
                        // In-memory adatbázis kontextus hozzáadása
                        services.AddDbContext<TravelAgencyContext>(options =>
                            options.UseInMemoryDatabase("TravelAgencyIntegrationTest"));

                        var sp = services.BuildServiceProvider();
                        using (var serviceScope = sp.CreateScope())
                        {
                            // Adatbázis inicializálása
                            var dbContext = serviceScope.ServiceProvider.GetRequiredService<TravelAgencyContext>();
                            dbContext.Database.EnsureCreated();
                            dbContext.Cities.AddRange(CityData);
                            dbContext.Buildings.AddRange(BuildingData);
                            await dbContext.SaveChangesAsync();

                            var adminUser = new User
                            {
                                UserName = "teszt@example.com",
                                Name = "Teszt Elek",
                                Email = "teszt@example.com",
                                PhoneNumber = "+36123456789",
                                Address = "Próba utca 42."
                            };
                            var adminPassword = "Teszt123!";
                            var adminRole = new IdentityRole("administrator");

                            var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();
                            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                            await userManager.CreateAsync(adminUser, adminPassword);
                            await roleManager.CreateAsync(adminRole);
                            await userManager.AddToRoleAsync(adminUser, adminRole.Name!);
                        }
                    });
                });
            
            _client = _server.CreateClient(); // kliens példányosítása
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _persistence = new TravelAgencyServicePersistence(_client); // kliens oldali perzisztencia réteg példányosítása
        }

        public void Dispose()
        {
            using (var serviceScope = _server.Services.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<TravelAgencyContext>();
                dbContext.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async void GetBuildingTest()
        {
            IEnumerable<BuildingDto> result = await _persistence.ReadBuildingsAsync();

            // Assert
            Assert.Equal(_buildingDtos, result);
        }

        [Fact]
        public async Task CreateBuildingTestUnauthorized()
        {
            var newBuilding = new BuildingDto
            {
                City = _cityDtos.First(),
                Name = "TESTBUILDING4",
                SeaDistance = 1000,
                Shore = ShoreTypeDto.Rocky,
                Features = new FeatureDto[]
                {
                    new FeatureDto {Id = (int)Feature.Garden, IsAvailable = true},
                    new FeatureDto {Id = (int)Feature.SwimmingPool, IsAvailable = true},
                },
                Comment = "Test Building 4"
            };

            bool result = await _persistence.CreateBuildingAsync(newBuilding); ;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateBuildingTestAuthorized()
        {
            var newBuilding = new BuildingDto
            {
                City = _cityDtos.First(),
                Name = "TESTBUILDING4",
                SeaDistance = 1000,
                Shore = ShoreTypeDto.Rocky,
                Features = new FeatureDto[]
                {
                    new FeatureDto {Id = (int)Feature.Garden, IsAvailable = true},
                    new FeatureDto {Id = (int)Feature.SwimmingPool, IsAvailable = true},
                },
                Comment = "Test Building 4"
            };

            await _persistence.LoginAsync("teszt@example.com", "Teszt123!");
            bool result = await _persistence.CreateBuildingAsync(newBuilding);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteBuildingTestUnauthorized()
        {
            var deleteBuilding = _buildingDtos.First();

            bool result = await _persistence.DeleteBuildingAsync(deleteBuilding);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteBuildingTestAuthorized()
        {
            var deleteBuilding = _buildingDtos.First();

            await _persistence.LoginAsync("teszt@example.com", "Teszt123!");
            bool result = await _persistence.DeleteBuildingAsync(deleteBuilding);

            // Assert
            Assert.True(result);
        }
    }
}
