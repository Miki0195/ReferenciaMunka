using Xunit;
using Bunit;
using Moq;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using ELTE.Cinema.Blazor.WebAssembly.ViewModels;
using ELTE.Cinema.Blazor.WebAssembly.Services;
using ELTE.Cinema.Blazor.WebAssembly.Pages.Reservation;

namespace ELTE.Cinema.Blazor.ComponentTests;

public class ScreeningReservationTests : IDisposable
{
    private readonly TestContext _context = new();
    private readonly Mock<IReservationService> _reservationServiceMock = new();
    private readonly Mock<IJSRuntime> _jsRuntimeMock = new();

    // Test data
    private readonly RoomViewModel _testRoom;

    private readonly List<SeatViewModel> _testSeats = [];

    public ScreeningReservationTests()
    {
        // Setup test data
        _testRoom = new()
        {
            Id = 1,
            Name = "Test Room",
            Rows = 5,
            Columns = 8
        };
        
        for (int row = 1; row <= _testRoom.Rows; row++)
        {
            for (int col = 1; col <= _testRoom.Columns; col++)
            {
                _testSeats.Add(new SeatViewModel
                {
                    Id = (row - 1) * _testRoom.Columns + col,
                    Row = row,
                    Column = col,
                    Status = SeatStatusViewModel.Free,
                    IsSelected = false
                });
            }
        }

        _testSeats[0].Status = SeatStatusViewModel.Reserved;
        _testSeats[1].Status = SeatStatusViewModel.Sold;
        _testSeats[0].ReservationId = 1;

        // Setup mocks and register services
        _reservationServiceMock
            .Setup(x => x.GetSeatsByScreeningAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((_testRoom, _testSeats));

        _context.Services.AddSingleton<IReservationService>(_reservationServiceMock.Object);
        _context.Services.AddSingleton<IJSRuntime>(_jsRuntimeMock.Object);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public void RoomDisplay_ShouldShowCorrectNumberOfSeats()
    {
        // Arrange
        var cut = _context.RenderComponent<ScreeningReservation>(
            parameters => parameters
                .Add(p => p.RoomId, 1)
                .Add(p => p.ScreeningId, 1)
        );

        // Assert
        var seatElements = cut.FindAll("tbody td[data-seatRow]");
        Assert.Equal(_testRoom.Rows * _testRoom.Columns, seatElements.Count);

        var tableRows = cut.FindAll("tbody tr");
        Assert.Equal(_testRoom.Rows, tableRows.Count);
    }

    [Fact]
    public void SeatDisplay_ShouldHaveCorrectCssClassBasedOnStatus()
    {
        // Arrange
        var cut = _context.RenderComponent<ScreeningReservation>(
            parameters => parameters
                .Add(p => p.RoomId, 1)
                .Add(p => p.ScreeningId, 1)
        );

        // Assert
        var reservedSeat = cut.Find("td[data-status='Reserved']");
        Assert.Contains("bg-warning", reservedSeat.ClassList);

        var soldSeat = cut.Find("td[data-status='Sold']");
        Assert.Contains("bg-success", soldSeat.ClassList);

        var freeSeat = cut.Find("td[data-status='Free']");
        Assert.Contains("bg-light", freeSeat.ClassList);
    }

    [Fact]
    public void LoadSelectedSeatData_WhenSeatClicked_ShouldShowSeatDetails()
    {
        // Arrange
        var selectedSeat = _testSeats.First(s => s.Status == SeatStatusViewModel.Reserved);
        var reservation = new ReservationViewModel
        {
            Id = 1,
            Name = "John Doe",
            Email = "test@example.com",
            Phone = "+123456789",
            Comment = "Test reservation",
            Seats = [selectedSeat]
        };

        var seatDetail = new SeatDetailViewModel
        {
            Seat = selectedSeat,
            Reservation = reservation
        };

        _reservationServiceMock.Setup(x => x.LoadSelectedSeatDataAsync(It.Is<SeatViewModel>(
                s => s.Id == selectedSeat.Id)))
            .ReturnsAsync(seatDetail);

        // Act
        var cut = _context.RenderComponent<ScreeningReservation>(
            parameters => parameters
                .Add(p => p.RoomId, 1)
                .Add(p => p.ScreeningId, 1)
        );

        var reservedSeatElement = cut.Find("td[data-status='Reserved']");
        reservedSeatElement.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var cardTitle = cut.Find(".card-title");
            Assert.Equal($"Row: {selectedSeat.Row} - Column: {selectedSeat.Column}", cardTitle.TextContent);

            var reservationName = cut.Find("p:contains('Name:')");
            Assert.Contains(reservation.Name, reservationName.TextContent);

            var reservationEmail = cut.Find("p:contains('Email:')");
            Assert.Contains(reservation.Email, reservationEmail.TextContent);
        });
    }

    [Fact]
    public void SellButton_WhenClicked_ShouldCallServiceAndUpdateUI()
    {
        // Arrange
        var freeSeat = _testSeats.First(s => s.Status == SeatStatusViewModel.Free);
        var seatDetail = new SeatDetailViewModel
        {
            Seat = freeSeat,
            Reservation = null
        };

        _reservationServiceMock
            .Setup(x => x.LoadSelectedSeatDataAsync(
                It.Is<SeatViewModel>(s => s.Id == freeSeat.Id)))
            .ReturnsAsync(seatDetail);

        var updatedSeat = new SeatViewModel
        {
            Id = freeSeat.Id,
            Row = freeSeat.Row,
            Column = freeSeat.Column,
            Status = SeatStatusViewModel.Sold,
            IsSelected = false
        };

        _reservationServiceMock.Setup(x => x.SellSeatAsync(
                It.IsAny<int>(),
                It.Is<SeatViewModel>(s => s.Id == freeSeat.Id)))
            .ReturnsAsync(updatedSeat);

        // Act
        var cut = _context.RenderComponent<ScreeningReservation>(
            parameters => parameters
                .Add(p => p.RoomId, 1)
                .Add(p => p.ScreeningId, 1)
        );

        // Find and click on a free seat
        var freeSeatElement = cut.Find("td[data-status='Free']");
        freeSeatElement.Click();

        // Click the sell button after the seat details are shown
        cut.WaitForAssertion(() =>
        {
            var sellButton = cut.Find("button.btn-success");
            sellButton.Click();
        });

        // Find modal and click confirm, the show css class confirms that the modal is shown
        var modelConfirmButton = cut.Find(".modal.show button.btn-danger");
        modelConfirmButton.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            _reservationServiceMock.Verify(x => x.SellSeatAsync(
                    It.IsAny<int>(),
                    It.Is<SeatViewModel>(s => s.Id == freeSeat.Id)),
                Times.Once);
        });
    }

    [Fact]
    public void DeleteReservation_WhenClicked_ShouldCallServiceAndReloadData()
    {
        // Arrange
        var reservedSeat = _testSeats.First(s => s.Status == SeatStatusViewModel.Reserved);
        var reservation = new ReservationViewModel
        {
            Id = 1,
            Name = "John Doe",
            Email = "test@example.com",
            Phone = "+123456789",
            Comment = "Test reservation",
            Seats = [reservedSeat]
        };

        var seatDetail = new SeatDetailViewModel
        {
            Seat = reservedSeat,
            Reservation = reservation
        };

        _reservationServiceMock.Setup(x => x.LoadSelectedSeatDataAsync(It.Is<SeatViewModel>(
                s => s.Id == reservedSeat.Id)))
            .ReturnsAsync(seatDetail);

        _reservationServiceMock.Setup(x => x.DeleteReservationAsync(It.Is<int>(id => id == reservation.Id)))
            .Returns(Task.CompletedTask);

        // Act
        var cut = _context.RenderComponent<ScreeningReservation>(
            parameters => parameters
                .Add(p => p.RoomId, 1)
                .Add(p => p.ScreeningId, 1)
        );

        // Find and click on a reserved seat
        var reservedSeatElement = cut.Find("td[data-status='Reserved']");
        reservedSeatElement.Click();

        // Click the delete button after the reservation details are shown
        cut.WaitForAssertion(() =>
        {
            // var deleteButton = cut.Find("button.btn-danger");
            var deleteButton = cut.Find("[data-testid='delete-button']");
            deleteButton.Click();
        });

        // Find modal and click confirm, the show css class confirms that the modal is shown
        var modelConfirmButton = cut.Find(".modal.show button.btn-danger");
        modelConfirmButton.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            _reservationServiceMock.Verify(x => x.DeleteReservationAsync(It.Is<int>(id => id == reservation.Id)),
                Times.Once);
            _reservationServiceMock.Verify(x => x.GetSeatsByScreeningAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Exactly(2));
        });
    }
}