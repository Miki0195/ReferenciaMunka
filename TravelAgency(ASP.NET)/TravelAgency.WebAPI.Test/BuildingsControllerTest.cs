using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ELTE.TravelAgency.DataAccess;
using ELTE.TravelAgency.Shared.Models;
using ELTE.TravelAgency.DataAccess.Models;
using ELTE.TravelAgency.WebAPI.Controllers;
using ELTE.TravelAgency.DataAccess.Services;
using ELTE.TravelAgency.WebAPI;
using AutoMapper;
using Xunit;

namespace ELTE.TravelAgency.WebAPI.Test
{
	public class BuildingsControllerTest : IDisposable
	{
		private readonly TravelAgencyContext _context;
		private readonly BuildingsController _controller;
		private readonly List<BuildingDto> _buildingDtos;
        private readonly List<CityDto> _cityDtos;

        public BuildingsControllerTest()
		{
			var options = new DbContextOptionsBuilder<TravelAgencyContext>()
				.UseInMemoryDatabase("TravelAgencyTest")
				.Options;

			_context = new TravelAgencyContext(options);
			_context.Database.EnsureCreated();

            // függőségek inicializációja
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
            var mapper = mapperConfig.CreateMapper();

            var buildingsService = new BuildingService(_context);
            _controller = new BuildingsController(buildingsService, mapper);

            // adatok inicializációja
            var cityData = new List<City>
			{
				new City { Name = "TESTCITY" }
			};
			_context.Cities.AddRange(cityData);

			var buildingData = new List<Building>
			{
				new Building
                {
                    City = cityData[0], 
                    Name = "TESTBUILDING1", 
                    SeaDistance = 1, 
                    Shore = ShoreType.Rocky, 
                    Features = Feature.CoastService | Feature.MainRoad,
					Comment = "Test Building 1"
                },
				new Building
                {
                    City = cityData[0], 
                    Name = "TESTBUILDING2", 
                    SeaDistance = 10, 
                    Shore = ShoreType.Gravelly,
					Features = Feature.None,
                    Comment = "Test Building 2"
				},
				new Building
                {
                    City = cityData[0], 
                    Name = "TESTBUILDING3", 
                    SeaDistance = 100, 
                    Shore = ShoreType.Sandy,
					Features = Feature.PrivateParking,
                    Comment = "Test Building 3"
				}
			};
			_context.Buildings.AddRange(buildingData);
			_context.SaveChanges();

			// DTO-k mappelése későbbi ellenőrzésekhez
			_buildingDtos = mapper.Map<List<BuildingDto>>(buildingData);
            _cityDtos = mapper.Map<List<CityDto>>(cityData);
        }

		public void Dispose()
		{
			_context.Database.EnsureDeleted();
			_context.Dispose();
		}

		[Fact]
		public async void GetBuildingTest()
		{
			var result = await _controller.GetBuildings(null);

			// válasz sikerességének, típusának és tartalmának ellenőrzése
			var objectResult = Assert.IsType<OkObjectResult>(result);
			var model = Assert.IsAssignableFrom<IEnumerable<BuildingDto>>(objectResult.Value);
			Assert.Equal(_buildingDtos, model);
		}


        [Fact]
        public async Task CreateBuildingTest()
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

            var result = await _controller.PostBuilding(newBuilding);

            // Assert
            var objectResult = Assert.IsType<CreatedAtActionResult>(result);
            var model = Assert.IsAssignableFrom<BuildingDto>(objectResult.Value);
            Assert.Equal(_buildingDtos.Count + 1, _context.Buildings.Count());
            Assert.Equal(newBuilding, model);
        }

        [Fact]
        public async Task DeleteBuildingTest()
        {
            int deletedId = _buildingDtos.First().Id;
            var result = await _controller.DeleteBuilding(deletedId);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Equal(_buildingDtos.Count - 1, _context.Buildings.Count());
            Assert.DoesNotContain(deletedId, _context.Buildings.Select(b => b.Id));
        }
    }
}
