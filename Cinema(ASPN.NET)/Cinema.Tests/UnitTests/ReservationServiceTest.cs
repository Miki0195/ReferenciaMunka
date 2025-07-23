using ELTE.Cinema.DataAccess;
using ELTE.Cinema.DataAccess.Config;
using ELTE.Cinema.DataAccess.Exceptions;
using ELTE.Cinema.DataAccess.Models;
using ELTE.Cinema.DataAccess.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace ELTE.Cinema.Tests.UnitTests;

public class ReservationsServiceTests : IDisposable
{
    private readonly CinemaDbContext _context;
    private readonly ReservationsService _reservationsService;
    private readonly Mock<IUsersService> _mockUserService;

    public ReservationsServiceTests()
    {
        // Configure in-memory database
        var options = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseInMemoryDatabase("TestReservationDatabase") // Unique database for each test
            .Options;

        _context = new CinemaDbContext(options);

        // Mock the UserService
        _mockUserService = new Mock<IUsersService>();

        // Configure ReservationSettings
        var reservationSettings = Options.Create(new ReservationSettings
        {
            MaximumNumberOfSeats = 6
        });

        // Mock email service
        Mock<IEmailsService> mockEmailService = new();

        // Initialize the ReservationsService
        _reservationsService = new ReservationsService(
            _context,
            reservationSettings,
            mockEmailService.Object,
            _mockUserService.Object);

        SeedDatabase();
    }

    #region Add

    [Fact]
    public async Task AddAsync_ThrowsNotFound_WhenScreeningNotExists()
    {
        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(2, 3) },
                new Seat { Position = new SeatPosition(2, 4) }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _reservationsService.AddAsync(99, reservation));
    }

    [Fact]
    public async Task AddAsync_ThrowsInvalidData_WhenDuplicatePositions()
    {
        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(2, 3) },
                new Seat { Position = new SeatPosition(2, 3) }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidDataException>(() => _reservationsService.AddAsync(1, reservation));
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentException_WhenAlreadyReserved()
    {
        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(2, 3) },
                new Seat { Position = new SeatPosition(2, 4) }
            }
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _reservationsService.AddAsync(1, reservation));
    }

    [Fact]
    public async Task AddAsync_ThrowsArgumentException_WhenExceedingSeatLimit()
    {
        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            Seats = Enumerable.Range(1, 7).Select(i => new Seat
            {
                Position = new SeatPosition(i, 1)
            }).ToList()
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _reservationsService.AddAsync(1, reservation));
    }

    [Fact]
    public async Task AddAsync_AddsReservation()
    {
        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(2, 3), ScreeningId = 1},
                new Seat { Position = new SeatPosition(2, 4), ScreeningId = 1}
            }
        };

        // Act
        await _reservationsService.AddAsync(1, reservation);

        // Assert
        var reservations = await _context.Reservations.ToListAsync();
        Assert.Single(reservations);
    }

    #endregion

    #region Get

    [Fact]
    public async Task GetAllReservationsAsync_ReturnsAllReservationsForAdmin()
    {
        // Arrange
        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            UserId = "user1",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(1, 3) },
                new Seat { Position = new SeatPosition(1, 4) }
            }
        };

        var reservation2 = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            UserId = "user2",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(2, 3) },
                new Seat { Position = new SeatPosition(2, 4) }
            }
        };

        _context.Reservations.AddRange([reservation, reservation2]);
        
        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.IsCurrentUserAdmin()).Returns(true);

        // Act
        var reservations = await _reservationsService.GetAllReservationsAsync();

        // Assert
        Assert.NotEmpty(reservations);
        Assert.Equal(2, reservations.Count);
    }

    [Fact]
    public async Task GetAllReservationsAsync_ReturnsOnlyOwnReservations()
    {
        // Arrange
        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            UserId = "user1",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(1, 3) },
                new Seat { Position = new SeatPosition(1, 4) }
            }
        };

        var reservation2 = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            UserId = "user2",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(2, 3) },
                new Seat { Position = new SeatPosition(2, 4) }
            }
        };

        _context.Reservations.AddRange([reservation, reservation2]);

        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.IsCurrentUserAdmin()).Returns(false);
        _mockUserService.Setup(x => x.GetCurrentUserId()).Returns("user1");

        // Act
        var reservations = await _reservationsService.GetAllReservationsAsync();

        // Assert
        Assert.NotEmpty(reservations);
        Assert.Single(reservations);
        Assert.Equal("user1", reservations.First().UserId);
    }

    [Fact]
    public async Task GetAllReservationByIdAsync_ReturnsAnyForAdmin()
    {
        // Arrange
        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            UserId = "user1",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(2, 3) },
                new Seat { Position = new SeatPosition(2, 4) }
            }
        };

        _context.Reservations.Add(reservation);

        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.IsCurrentUserAdmin()).Returns(true);

        // Act
        var savedReservation = await _reservationsService.GetByIdAsync(1);

        // Assert
        Assert.NotNull(savedReservation);
    }

    [Fact]
    public async Task GetAllReservationByIdAsync_ThrowsAccessDenied_WhenNotAdmin()
    {
        // Arrange
        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            UserId = "user1",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(2, 3) },
                new Seat { Position = new SeatPosition(2, 4) }
            }
        };

        _context.Reservations.Add(reservation);

        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.IsCurrentUserAdmin()).Returns(false);
        _mockUserService.Setup(x => x.GetCurrentUserId()).Returns("user2");

        // Act & Assert
        await Assert.ThrowsAsync<AccessViolationException>(() => _reservationsService.GetByIdAsync(1));
    }

    [Fact]
    public async Task GetAllReservationByIdAsync_ReturnsWhenSameUser()
    {
        // Arrange
        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            UserId = "user1",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(2, 3) },
                new Seat { Position = new SeatPosition(2, 4) }
            }
        };

        _context.Reservations.Add(reservation);

        await _context.SaveChangesAsync();

        _mockUserService.Setup(x => x.IsCurrentUserAdmin()).Returns(false);
        _mockUserService.Setup(x => x.GetCurrentUserId()).Returns("user1");

        // Act
        var savedReservation = await _reservationsService.GetByIdAsync(1);

        // Assert
        Assert.NotNull(savedReservation);
    }

    #endregion

    #region Cancel

    [Fact]
    public async Task CancelAsync_ThrowsAccessDenied_WhenReservationNotExists()
    {
        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _reservationsService.CancelAsync(1));
    }

    [Fact]
    public async Task CancelAsync_ThrowsAccessDenied_WhenNotOwnReservation()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetCurrentUserId()).Returns("user2");

        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            UserId = "user1",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(2, 3) },
                new Seat { Position = new SeatPosition(2, 4) }
            }
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<AccessViolationException>(() => _reservationsService.CancelAsync(1));
    }

    [Fact]
    public async Task CancelAsync_RemovesOwnReservation()
    {
        // Arrange
        _mockUserService.Setup(x => x.GetCurrentUserId()).Returns("user1");

        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            UserId = "user1",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(2, 3), ScreeningId = 1 },
                new Seat { Position = new SeatPosition(2, 4), ScreeningId = 1 }
            }
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Act
        await _reservationsService.CancelAsync(1);

        // Assert
        reservation = await _context.Reservations.FindAsync(1);
        Assert.Null(reservation);
    }

    [Fact]
    public async Task CancelAsync_RemovesAnyReservation_WhenAdmin()
    {
        // Arrange
        _mockUserService.Setup(x => x.IsCurrentUserAdmin()).Returns(true);
        
        var reservation = new Reservation
        {
            Email = "test@test.com",
            Name = "Test Reservation",
            Phone = "06301111111",
            UserId = "user1",
            Seats = new List<Seat>
            {
                new Seat { Position = new SeatPosition(2, 3), ScreeningId = 1 },
                new Seat { Position = new SeatPosition(2, 4), ScreeningId = 1 }
            }
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Act
        await _reservationsService.CancelAsync(1);

        // Assert
        reservation = await _context.Reservations.FindAsync(1);
        Assert.Null(reservation);
    }

    #endregion

    #region Helper

    private void SeedDatabase()
    {
        var screening = new Screening
        {
            Movie = new Movie { Id = 1, Director = "Test Director", Length = 120, Year = 2024, Title = "Test Movie", CreatedAt = DateTime.UtcNow, Image = [], Synopsis = ""},
            Room = new Room { Id = 1, Rows = 10, Columns = 10, Name = "Room 1", CreatedAt = DateTime.UtcNow },
            Seats = new List<Seat>(),
            CreatedAt = DateTime.UtcNow,
            StartsAt = DateTime.Now.AddDays(1),    
        };

        _context.Screenings.Add(screening);

        _context.SaveChanges();
    }

    #endregion

    public void Dispose()
    {
        _context.Database.EnsureDeleted(); // Deletes the in-memory database
        _context.Dispose();
    }
}