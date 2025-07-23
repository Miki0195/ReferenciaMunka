using AutoMapper;
using ELTE.Cinema.DataAccess;
using ELTE.Cinema.DataAccess.Exceptions;
using ELTE.Cinema.DataAccess.Models;
using ELTE.Cinema.DataAccess.Services;
using ELTE.Cinema.Shared.Models;
using ELTE.Cinema.WebAPI.Controllers;
using ELTE.Cinema.WebAPI.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELTE.Cinema.Tests.ControllerTests;

public class ScreeningsControllerIntegrationTests: IDisposable
{
    private readonly CinemaDbContext _context;
    private readonly ScreeningsController _controller;

    public ScreeningsControllerIntegrationTests()
    {
        // Set up the in-memory database
        var options = new DbContextOptionsBuilder<CinemaDbContext>()
            .UseInMemoryDatabase("TestScreeningDatabase")
            .Options;
        _context = new CinemaDbContext(options);
        
        // Initialize dependencies
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile(new MappingProfile()));
        var mapper = mapperConfig.CreateMapper();
        
        var screeningService = new ScreeningsService(_context, new RoomsService(_context), new MoviesService(_context));

        // Initialize the controller
        _controller = new ScreeningsController(mapper, screeningService);
        
        //Init database
        SeedDatabase();
    }

    #region Create

    [Fact]
    public async Task CreateScreening_ThrowsNotFound_WhenRoomDoesNotExist()
    {
        // Arrange
        var requestDto = new ScreeningRequestDto
        {
            MovieId = 1,
            RoomId = 999, // Room does not exist
            StartsAt = DateTime.UtcNow.AddHours(1)
        };
        
        await _context.SaveChangesAsync();

        // Act && Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _controller.CreateScreening(requestDto));
    }
    
    [Fact]
    public async Task CreateScreening_ThrowsNotFound_WhenRoomDeleted()
    {
        // Arrange
        _context.Remove(_context.Movies.First());
        
        var requestDto = new ScreeningRequestDto
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };
        
        await _context.SaveChangesAsync();

        // Act && Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _controller.CreateScreening(requestDto));
    }

    [Fact]
    public async Task CreateScreening_ThrowsNotFound_WhenMovieDoesNotExist()
    {
        // Arrange
        var requestDto = new ScreeningRequestDto
        {
            MovieId = 999, // Movie does not exist
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };

        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _controller.CreateScreening(requestDto));
    }
    
    [Fact]
    public async Task CreateScreening_ThrowsNotFound_WhenMovieDeleted()
    {
        // Arrange
        _context.Remove(_context.Rooms.First());
        
        var requestDto = new ScreeningRequestDto
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };
        
        await _context.SaveChangesAsync();

        // Act && Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _controller.CreateScreening(requestDto));
    }

    [Fact]
    public async Task CreateScreening_ThrowsInvalidData_WhenScheduleOverlaps()
    {
        // Arrange
        var existingScreening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };
        
        _context.Screenings.Add(existingScreening);
        await _context.SaveChangesAsync();

        var overlappingRequestDto = new ScreeningRequestDto
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = existingScreening.StartsAt.AddMinutes(30) // Overlaps with the existing screening
        };

        // Act && Assert
        await Assert.ThrowsAsync<InvalidDataException>(() => _controller.CreateScreening(overlappingRequestDto));
    }

    [Fact]
    public async Task CreateScreening_AddsScreeningToDatabase()
    {
        // Arrange
        var requestDto = new ScreeningRequestDto
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };

        // Act
        var result = await _controller.CreateScreening(requestDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var responseDto = Assert.IsType<ScreeningResponseDto>(createdResult.Value);
        Assert.Equal(requestDto.MovieId, responseDto.Movie.Id);
        Assert.Equal(requestDto.RoomId, responseDto.Room.Id);

        // Verify the data in the database
        var screenings = _context.Screenings.ToList();
        Assert.Single(screenings);
        Assert.Equal(requestDto.MovieId, screenings[0].MovieId);
        Assert.Equal(requestDto.RoomId, screenings[0].RoomId);
    }

    #endregion

    #region Get

    [Fact]
    public async Task GetScreenings_ReturnsScreeningList()
    {
        // Arrange
        _context.Screenings.AddRange([
            new Screening { MovieId = 1, RoomId = 1, StartsAt = DateTime.UtcNow.AddHours(1) },
            new Screening { MovieId = 2, RoomId = 2, StartsAt = DateTime.UtcNow.AddHours(2) }
        ]);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetScreenings(null, null, null, null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDtos = Assert.IsType<List<ScreeningResponseDto>>(okResult.Value);
        Assert.Equal(2, responseDtos.Count);
    }
    
    [Fact]
    public async Task GetScreenings_ReturnsScreeningListForMovie()
    {
        // Arrange
        _context.Screenings.AddRange([
            new Screening { MovieId = 1, RoomId = 1, StartsAt = DateTime.UtcNow.AddHours(1) },
            new Screening { MovieId = 2, RoomId = 2, StartsAt = DateTime.UtcNow.AddHours(2) }
        ]);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetScreenings(null, null, 2, null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDtos = Assert.IsType<List<ScreeningResponseDto>>(okResult.Value);
        Assert.Single(responseDtos);
    }
    
    [Fact]
    public async Task GetScreenings_ReturnsScreeningListForRoom()
    {
        // Arrange
        _context.Screenings.AddRange([
            new Screening { MovieId = 1, RoomId = 1, StartsAt = DateTime.UtcNow.AddHours(1) },
            new Screening { MovieId = 2, RoomId = 2, StartsAt = DateTime.UtcNow.AddHours(2) }
        ]);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetScreenings(null, null, null, 1, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDtos = Assert.IsType<List<ScreeningResponseDto>>(okResult.Value);
        Assert.Single(responseDtos);
    }
    
    [Fact]
    public async Task GetScreenings_ReturnsScreeningListForInterval()
    {
        // Arrange
        _context.Screenings.AddRange([
            new Screening { MovieId = 1, RoomId = 1, StartsAt = DateTime.UtcNow.AddHours(1) },
            new Screening { MovieId = 2, RoomId = 2, StartsAt = DateTime.UtcNow.AddHours(5) }
        ]);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetScreenings(null, null, null, null, DateTime.UtcNow.AddHours(1).AddMinutes(-1), DateTime.UtcNow.AddHours(4));

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDtos = Assert.IsType<List<ScreeningResponseDto>>(okResult.Value);
        Assert.Single(responseDtos);
    }
    
    [Fact]
    public async Task GetScreenings_ReturnsScreeningListPaged()
    {
        // Arrange
        _context.Screenings.AddRange([
            new Screening { MovieId = 1, RoomId = 1, StartsAt = DateTime.UtcNow.AddHours(1) },
            new Screening { MovieId = 2, RoomId = 2, StartsAt = DateTime.UtcNow.AddHours(5) }
        ]);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetScreenings(1, 1, null, null, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDtos = Assert.IsType<List<ScreeningResponseDto>>(okResult.Value);
        Assert.Single(responseDtos);
    }
    
    [Fact]
    public async Task GetScreenings_ReturnsScreeningWithAllParameters()
    {
        // Arrange
        _context.Screenings.AddRange([
            new Screening { MovieId = 1, RoomId = 1, StartsAt = DateTime.UtcNow.AddHours(1) },
            new Screening { MovieId = 2, RoomId = 2, StartsAt = DateTime.UtcNow.AddHours(5) }
        ]);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetScreenings(0, 2, 1, 1, DateTime.UtcNow.AddHours(1).AddMinutes(-1), DateTime.UtcNow.AddHours(5).AddMinutes(5));

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDtos = Assert.IsType<List<ScreeningResponseDto>>(okResult.Value);
        Assert.Single(responseDtos);
    }

    [Fact]
    public async Task GetScreeningById_ThrowsNotFound_WhenDoesNotExist()
    {
        // Act && Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _controller.GetScreeningById(1));
    }
    
    [Fact]
    public async Task GetScreeningById_ReturnsScreening_WhenExists()
    {
        // Arrange
        var screening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };

        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetScreeningById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var responseDto = Assert.IsType<ScreeningResponseDto>(okResult.Value);
        Assert.Equal(screening.Id, responseDto.Id);
    }

    #endregion
    
    #region Update
    [Fact]
    public async Task UpdateScreening_ThrowsNotFound_WhenScreeningDoesNotExist()
    {
        // Arrange
        var updateDto = new ScreeningRequestDto
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };

        // Act && Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _controller.UpdateScreening(999, updateDto));
    }

    [Fact]
    public async Task UpdateScreening_ThrowsNotFound_WhenRoomDoesNotExist()
    {
        // Arrange
        var screening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };

        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();

        var updateDto = new ScreeningRequestDto
        {
            MovieId = 1, 
            RoomId = 999, // Room does not exist
            StartsAt = DateTime.UtcNow.AddHours(2)
        };

        // Act && Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _controller.UpdateScreening(1, updateDto));
    }
    
    [Fact]
    public async Task UpdateScreening_ThrowsNotFound_WhenRoomDeleted()
    {
        // Arrange
        _context.Remove(_context.Rooms.First());
        
        var screening = new Screening
        {
            MovieId = 1,
            RoomId = 2,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };

        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();

        var updateDto = new ScreeningRequestDto
        {
            MovieId = 1, 
            RoomId = 1, 
            StartsAt = DateTime.UtcNow.AddHours(2)
        };

        // Act && Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _controller.UpdateScreening(1, updateDto));
    }
    
    [Fact]
    public async Task UpdateScreening_ThrowsInvalidData_WhenMovieIdChanged()
    {
        // Arrange
        var screening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };

        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();

        var updateDto = new ScreeningRequestDto
        {
            MovieId = 2, //Different movie
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(2)
        };

        // Act && Assert
        await Assert.ThrowsAsync<InvalidDataException>(() => _controller.UpdateScreening(1, updateDto));
    }

    [Fact]
    public async Task UpdateScreening_ThrowsInvalidData_WhenScheduleOverlaps()
    {
        // Arrange
        var existingScreening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };
        var overlappingScreening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(2)
        };
        
        _context.Screenings.AddRange(existingScreening, overlappingScreening);
        await _context.SaveChangesAsync();

        var updateDto = new ScreeningRequestDto
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = overlappingScreening.StartsAt // Overlaps with an existing screening
        };

        // Act && Assert
        await Assert.ThrowsAsync<InvalidDataException>(() => _controller.UpdateScreening(1, updateDto));
    }
    
    [Fact]
    public async Task UpdateScreening_ThrowsInvalidData_WhenScheduleOverlapsInDifferentRoom()
    {
        // Arrange
        var existingScreening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };
        var overlappingScreening = new Screening
        {
            MovieId = 1,
            RoomId = 2,
            StartsAt = DateTime.UtcNow.AddHours(3)
        };
        
        _context.Screenings.AddRange(existingScreening, overlappingScreening);
        await _context.SaveChangesAsync();

        var updateDto = new ScreeningRequestDto
        {
            MovieId = 1,
            RoomId = 2,
            StartsAt = overlappingScreening.StartsAt // Overlaps with an existing screening
        };

        // Act && Assert
        await Assert.ThrowsAsync<InvalidDataException>(() => _controller.UpdateScreening(1, updateDto));
    }

    [Fact]
    public async Task UpdateScreening_UpdatesScreeningInDatabase()
    {
        // Arrange
        var screening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };

        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();
        
        DetachAllEntities();

        var updateDto = new ScreeningRequestDto
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(_context.Movies.First().Length + 5)
        };

        // Act
        var result = await _controller.UpdateScreening(1, updateDto);

        // Assert
        _ = Assert.IsType<OkObjectResult>(result);
        var updatedScreening = _context.Screenings.FirstOrDefault(s => s.Id == 1);
        Assert.NotNull(updatedScreening);
        Assert.Equal(updateDto.StartsAt, updatedScreening.StartsAt);
    }
    #endregion
    
    #region Delete
    
    [Fact]
    public async Task DeleteScreening_ThrowsNotFound_WhenScreeningDoesNotExist()
    {
        // Act && Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _controller.DeleteScreening(999));
    }
    
    [Fact]
    public async Task DeleteScreening_ThrowsInvalidOperation_WhenSeatsExists()
    {
        // Arrange
        var screening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            Seats = [new Seat
            {
                Position = new SeatPosition(1,1),
                Status = SeatStatus.Sold,
            }]
        };
        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();

        // Act && Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteScreening(screening.Id));
    }
    
    [Fact]
    public async Task DeleteScreening_RemovesScreeningFromDatabase()
    {
        // Arrange
        var screening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };

        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteScreening(1);

        // Assert
        Assert.IsType<NoContentResult>(result);
        Assert.Empty(_context.Screenings);
    }
    
    #endregion
    
    #region Seats

    [Fact]
    public async Task SellSeatsScreening_ThrowsNotFound_WhenScreeningDoesNotExist()
    {
        // Arrange
        var seatRequestDto = new SeatRequestDto
        {
           Row = 1,
           Column = 1
        };
        
        await _context.SaveChangesAsync();

        // Act && Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() => _controller.SellSeat(999, seatRequestDto));
    }
    
    [Fact]
    public async Task SellSeatsScreening_ThrowsArgumentOutOfRange_WhenInvalidRow()
    {
        // Arrange
        var screening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };

        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();
        
        var seatRequestDto = new SeatRequestDto
        {
            Row = 1000, //Row is bigger than configured in Room
            Column = 1
        };
        
        await _context.SaveChangesAsync();

        // Act && Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _controller.SellSeat(1, seatRequestDto));
    }
    
    [Fact]
    public async Task SellSeatsScreening_ThrowsArgumentOutOfRange_WhenInvalidColumn()
    {
        // Arrange
        var screening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };

        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();
        
        var seatRequestDto = new SeatRequestDto
        {
            Row = 1, 
            Column = 1000 //Column is bigger than configured in Room
        };
        
        await _context.SaveChangesAsync();

        // Act && Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _controller.SellSeat(1, seatRequestDto));
    }
    
    [Fact]
    public async Task SellSeatsScreening_ThrowsArgumentOutOfRange_WhenAlreadySold()
    {
        // Arrange
        var screening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1),
            Seats = [new Seat
            {
                Position = new SeatPosition(1,1),
                Status = SeatStatus.Sold
            }]
        };
        
        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();
        
        var seatRequestDto = new SeatRequestDto
        {
            Row = 1, 
            Column = 1 
        };
        
        await _context.SaveChangesAsync();

        // Act && Assert
        await Assert.ThrowsAsync<InvalidDataException>(() => _controller.SellSeat(1, seatRequestDto));
    }
    
    [Fact]
    public async Task SellSeatsScreening_SellSeat()
    {
        // Arrange
        var screening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1)
        };
        
        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();
        
        var seatRequestDto = new SeatRequestDto
        {
            Row = 1, 
            Column = 1 
        };
        
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.SellSeat(1, seatRequestDto);
        
        //Assert
        _ = Assert.IsType<OkObjectResult>(result);
        var seat = _context.Seats.FirstOrDefault(s => s.ScreeningId == screening.Id);
        Assert.NotNull(seat);
        Assert.Equal(seat.Position.Row, seatRequestDto.Row);
        Assert.Equal(seat.Position.Column, seatRequestDto.Column);
        Assert.Equal(seat.Status.ToString(), SeatStatus.Sold.ToString());
    }
    
    [Fact]
    public async Task GetSeats_ReturnsAllSeats()
    {
        // Arrange
        var screening = new Screening
        {
            MovieId = 1,
            RoomId = 1,
            StartsAt = DateTime.UtcNow.AddHours(1),
            Seats = [new Seat
            {
                Position = new SeatPosition(1,1),
                Status = SeatStatus.Sold
            }]
        };
        
        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync();
        
        //Act
        var result = await _controller.GetSeatsByScreening(1);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var seatDtos = Assert.IsType<List<SeatResponseDto>>(okResult.Value);
        Assert.Single(seatDtos);
    }
    #endregion

    #region Helpers

    private void SeedDatabase()
    {
        if (_context.Movies.Any() || _context.Rooms.Any())
            return; // Prevent duplicate seeding

        // Seed Rooms
        var rooms = new List<Room>
        {
            new() { Name = "Room A", Rows = 10, Columns = 12, CreatedAt = DateTime.UtcNow },
            new() { Name = "Room B", Rows = 15, Columns = 15, CreatedAt = DateTime.UtcNow },
            new() { Name = "Room C", Rows = 20, Columns = 20, CreatedAt = DateTime.UtcNow },
        };

        // Seed Movies
        var movies = new List<Movie>
        {
            new()
            {
                Title = "Inception",
                Year = 2010,
                Director = "Christopher Nolan",
                Synopsis = "A skilled thief is offered a chance to have his criminal history erased as payment for the implantation of another person's idea into a target's subconscious.",
                Length = 148,
                Image = [], // Placeholder image data
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Title = "The Matrix",
                Year = 1999,
                Director = "The Wachowskis",
                Synopsis = "A computer hacker learns about the true nature of his reality and his role in the war against its controllers.",
                Length = 136,
                Image = [], // Placeholder image data
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Title = "Interstellar",
                Year = 2014,
                Director = "Christopher Nolan",
                Synopsis = "A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival.",
                Length = 169,
                Image = [], // Placeholder image data
                CreatedAt = DateTime.UtcNow
            },
        };

        _context.Rooms.AddRange(rooms);
        _context.Movies.AddRange(movies);
        
        _context.SaveChanges();
        
        //Clear Change-tracker
        DetachAllEntities();
    }
    
    private void DetachAllEntities()
    {
        var trackedEntities = _context.ChangeTracker.Entries().ToList();
        foreach (var entry in trackedEntities)
        {
            entry.State = EntityState.Detached;
        }
    }

    #endregion
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted(); // Deletes the in-memory database
        _context.Dispose();
    }
}