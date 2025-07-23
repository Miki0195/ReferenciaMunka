using Bunit;
using Bunit.TestDoubles;
using ELTE.TravelAgency.Admin.Blazor.Components;
using ELTE.TravelAgency.Admin.Model;
using ELTE.TravelAgency.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ELTE.TravelAgency.Admin.Blazor.Test
{
    public class BuildingManagerTest : IDisposable
    {
        private readonly TestContext _context = new();
        private readonly Mock<ITravelAgencyModel> _modelMock = new();
        private readonly FakeNavigationManager _fakeNavigationManager;

        private readonly List<CityDto> _cityData;
        private readonly List<BuildingDto> _buildingData;

        public BuildingManagerTest()
        {
            // Initalize tets data
            _cityData = new List<CityDto>
            {
                new CityDto { Name = "TESTCITY" }
            };

            _buildingData = new List<BuildingDto>
            {
                new BuildingDto
                    {
                        City = _cityData[0],
                        Name = "TESTBUILDING1",
                        SeaDistance = 1,
                        Shore = ShoreTypeDto.Rocky,
                        Features = new FeatureDto[]
                        {
                             new FeatureDto {Id = 1, IsAvailable = true},
                             new FeatureDto {Id = 2, IsAvailable = true},
                        },
                        Comment = "Test Building 1"
                    },
                    new BuildingDto
                    {
                        City = _cityData[0],
                        Name = "TESTBUILDING2",
                        SeaDistance = 10,
                        Shore = ShoreTypeDto.Gravelly,
                        Features = new FeatureDto[0],
                        Comment = "Test Building 2"
                    },
                    new BuildingDto
                    {
                        City = _cityData[0],
                        Name = "TESTBUILDING3",
                        SeaDistance = 100,
                        Shore = ShoreTypeDto.Sandy,
                        Features = new FeatureDto[]
                        {
                             new FeatureDto {Id = 3, IsAvailable = true},
                        },
                        Comment = "Test Building 3"
                    }
            };

            // Setup mocks and register services
            _modelMock.Setup(m => m.Cities).Returns(_cityData);
            _modelMock.Setup(m => m.Buildings).Returns(_buildingData);
            _modelMock.Setup(m => m.DeleteBuilding(It.IsAny<BuildingDto>()))
                .Callback<BuildingDto>(building =>
                {
                    _buildingData.Remove(building);
                });
            _context.Services.AddSingleton<ITravelAgencyModel>(_modelMock.Object);

            _fakeNavigationManager = new FakeNavigationManager(_context);
            _context.Services.AddSingleton<NavigationManager>(_fakeNavigationManager);
        }

        public void Dispose()
        {
            _context.Dispose();
        }


        [Fact]
        public void ShowBuildingsTest()
        {
            // cut = Component Under Test
            var cut = _context.RenderComponent<BuildingManager>();

            var tableRows = cut.FindAll("table tr");
            Assert.Equal(_buildingData.Count + 1, tableRows.Count);

            foreach (var building in _buildingData)
            {
                Assert.Contains(cut.FindAll("table td"), td => td.TextContent.Contains(building.Name));
            }
        }

        [Fact]
        public void SelectBuildingTest()
        {
            bool buildingSelectCallbackRaised = false;

            // Pass parameters
            var cut = _context.RenderComponent<BuildingManager>(
                parameters => parameters
                    .Add(p => p.OnBuildingSelected, dto =>
                    {
                        buildingSelectCallbackRaised = true;
                    }));

            var firstDataRow = cut.FindAll("table tr").Skip(1).First();
            Assert.False(firstDataRow.ClassList.Contains("table-active"));

            firstDataRow.Click();

            // See: https://bunit.dev/docs/verification/async-assertion.html
            cut.WaitForAssertion(() =>
            {
                firstDataRow = cut.FindAll("table tr").Skip(1).First();
                Assert.True(firstDataRow.ClassList.Contains("table-active"));
                Assert.True(buildingSelectCallbackRaised);
            });
        }

        [Fact]
        public void NavigateToAddBuildingTest()
        {
            var cut = _context.RenderComponent<BuildingManager>();

            var addButton = cut.Find(".btn-primary");
            addButton.Click();

            cut.WaitForAssertion(() =>
            {
                // https://bunit.dev/docs/test-doubles/fake-navigation-manager.html
                Assert.Equal("buildings/add", _fakeNavigationManager.ToBaseRelativePath(_fakeNavigationManager.Uri));
            });
        }

        [Fact]
        public void DeleteBuildingTest()
        {
            var cut = _context.RenderComponent<BuildingManager>();

            var firstDataRow = cut.FindAll("table tr").Skip(1).First();
            Assert.False(firstDataRow.ClassList.Contains("table-active"));

            firstDataRow.Click();

            cut.WaitForAssertion(() =>
            {
                cut.Find("table tr.table-active");
            });

            int newBuildingCount = _buildingData.Count - 1;
            string deletedBuildingName = _buildingData[0].Name;

            var deleteButton = cut.Find(".btn-danger");
            deleteButton.Click();

            cut.WaitForAssertion(() =>
            {
                var tableRows = cut.FindAll("table tr");
                Assert.Equal(newBuildingCount + 1, tableRows.Count);

                Assert.DoesNotContain(cut.FindAll("table td"), td => td.TextContent.Contains(deletedBuildingName));
            });
        }
    }
}