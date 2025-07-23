using System;
using System.Collections.Generic;
using System.Linq;
using ELTE.TravelAgency.DataAccess;
using ELTE.TravelAgency.DataAccess.Models;
using ELTE.TravelAgency.Shared.Models;
using ELTE.TravelAgency.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Moq;
using Xunit;
using ELTE.TravelAgency.DataAccess.Services;

namespace ELTE.TravelAgency.WebAPI.Test
{
	public class BuildingsMockServiceTest
	{
        public static List<City> CityData = new List<City>
        {
            new City { Id = 1, Name = "TESTCITY" }
        };

        public static List<Building> BuildingData = new List<Building>
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

        private readonly BuildingsController _controller;
        private readonly List<BuildingDto> _buildingDtos;
        private readonly List<CityDto> _cityDtos;

        public BuildingsMockServiceTest()
		{
			// adatok inicializációja

            // szolgáltatás mockolása
            var serviceMock = new Mock<IBuildingService>();
            serviceMock.Setup(service => service.GetAllAsync()).ReturnsAsync(BuildingData);
            serviceMock.Setup(service => service.GetByCityAsync(It.IsAny<int>())).ReturnsAsync(
                (int cityId) => BuildingData.Where(building => building.CityId == cityId).ToList());

            // mockoljuk a hozzáadást is
            serviceMock.Setup(service => service.AddAsync(It.IsAny<Building>())).Returns(
                (Building building) =>
                {
                    BuildingData.Add(building);
                    return Task.CompletedTask;
                });

            // mockoljuk a törlést is
            serviceMock.Setup(service => service.DeleteAsync(It.IsAny<int>())).Returns(
                (int id) =>
                {
                    BuildingData.Remove(BuildingData.First(building => building.Id == id));
                    return Task.CompletedTask;
                });

            // függőségek inicializációja
            var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
            var mapper = mapperConfig.CreateMapper();

            _controller = new BuildingsController(serviceMock.Object, mapper);

            // DTO-k mappelése későbbi ellenőrzésekhez
            _buildingDtos = mapper.Map<List<BuildingDto>>(BuildingData);
            _cityDtos = mapper.Map<List<CityDto>>(CityData);
        }

		[Fact]
		public async Task GetBuildingTest()
		{
			var result = await _controller.GetBuildings(null);

			// Assert
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
            Assert.Equal(_buildingDtos.Count + 1, BuildingData.Count);
            Assert.Equal(newBuilding, model);
        }

        [Fact]
        public async Task DeleteBuildingTest()
        {
            int deletedId = _buildingDtos.First().Id;
            var result = await _controller.DeleteBuilding(deletedId);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Equal(_buildingDtos.Count - 1, BuildingData.Count);
            Assert.DoesNotContain(deletedId, BuildingData.Select(b => b.Id));
        }


    }
}
