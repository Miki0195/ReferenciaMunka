using ELTE.Cinema.DataAccess.Exceptions;
using ELTE.Cinema.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ELTE.Cinema.DataAccess.Services;

public class ScreeningsService : IScreeningsService
{
    private readonly CinemaDbContext _context;
    private readonly IRoomsService _roomsService;
    private readonly IMoviesService _moviesService;

    public ScreeningsService(CinemaDbContext context, IRoomsService roomsService, IMoviesService moviesService)
    {
        _context = context;
        _roomsService = roomsService;
        _moviesService = moviesService;
    }

    public async Task<IReadOnlyCollection<Screening>> GetForDateAsync(DateTime date)
    {
       return await _context.Screenings
            .Where(s => s.StartsAt.Date == date.Date)
            .OrderBy(s => s.StartsAt)
            .ToListAsync();
    }

    public async Task<Screening> GetByIdAsync(int id)
    {
        var screening = await _context.Screenings.FirstOrDefaultAsync(s => s.Id == id);
        if (screening is null)
            throw new EntityNotFoundException(nameof(screening));

        return screening;
    }

    public async Task<PaginatedResult<Screening>> GetAllAsync(int? movieId, int? roomId, int? page, int? pageSize, DateTime? from = null, DateTime? until = null)
    {
        var query = _context.Screenings.AsQueryable();
        if (from.HasValue)
        {
            query = query.Where(s => s.StartsAt >= from);
        }

        if (until.HasValue)
        {
            query = query.Where(s => s.StartsAt <= until);
        }

        query = query
            .Where(s =>
                (!movieId.HasValue || s.MovieId == movieId)
                && (!roomId.HasValue || s.RoomId == roomId))
            .OrderBy(s => s.StartsAt);

        var count = await query.CountAsync();

        if (page.HasValue && pageSize.HasValue)
        {
            query = query
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        var items = await query.ToListAsync();

        return new PaginatedResult<Screening>(
            count,
            pageSize ?? count,
            page ?? 0,
            count / pageSize ?? count,
            items
        );
    }

    public async Task AddAsync(Screening screening)
    {
       _ = await _roomsService.GetByIdAsync(screening.RoomId);
       var existingMovie = await _moviesService.GetByIdAsync(screening.MovieId);
       
       await CheckForOverlappingAsync(screening, existingMovie.Length);
       
       screening.CreatedAt = DateTime.UtcNow;
        
        try
        {
            await _context.Screenings.AddAsync(screening);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new SaveFailedException("Failed to create screening.", ex);
        }
    }

    public async Task UpdateAsync(Screening screening)
    {
        var existingScreening = await GetByIdAsync(screening.Id);
        
        if(existingScreening ==  null)
            throw new EntityNotFoundException(nameof(screening));
        
        if(existingScreening.MovieId != screening.MovieId)
            throw new InvalidDataException("Movie cannot be changed.");
        
        if (existingScreening.RoomId != screening.RoomId)
        {
            _ = await _roomsService.GetByIdAsync(screening.RoomId);
        }

        if (existingScreening.RoomId != screening.RoomId || existingScreening.StartsAt != screening.StartsAt)
        {
            var movie = await _moviesService.GetByIdAsync(screening.MovieId);
            await CheckForOverlappingAsync(screening, movie.Length);
        }
        
        screening.CreatedAt = existingScreening.CreatedAt;
        
        try
        {
            _context.Entry(existingScreening).State = EntityState.Detached;
            _context.Update(screening);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new SaveFailedException("Failed to update screening.", ex);
        }
    }

    public async Task DeleteAsync(int id)
    {
        var screening = await GetByIdAsync(id);
        
        if (screening.Seats.Count != 0)
            throw new InvalidOperationException("Screening cannot be deleted because some seats were already sold");
       
        try
        {
            _context.Remove(screening);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new SaveFailedException("Failed to delete screening.", ex);
        }
    }

    public async Task<List<Seat>> GetSeatsByScreeningAsync(int id)
    {
        var seats = await _context.Seats
            .Where(s => s.ScreeningId == id)
            .ToListAsync();

        return seats;
    }

    public async Task SellSeatForScreeningAsync(int id, Seat seat)
    {
        var screening = await _context.Screenings
            .Include(s => s.Seats)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (screening is null)
            throw new EntityNotFoundException(nameof(screening));
        
        var existingPosition = screening.Seats.FirstOrDefault(s => s.Position == seat.Position);
        if(existingPosition != null)
            throw new InvalidDataException($"Seat ({seat.Position.Row}, {seat.Position.Column}) is already sold.");
        
        if (seat.Position.Row < 1 || screening.Room.Rows < seat.Position.Row)
            throw new ArgumentOutOfRangeException(nameof(seat.Position), $"Invalid position: {seat.Position.Row}, {seat.Position.Column}.");
            
        if (seat.Position.Column < 1 || screening.Room.Columns < seat.Position.Column)
            throw new ArgumentOutOfRangeException(nameof(seat.Position), $"Invalid position: {seat.Position.Row}, {seat.Position.Column}.");

        seat.ScreeningId = screening.Id;
        seat.Status = SeatStatus.Sold;
            
        await _context.Seats.AddAsync(seat);
        await _context.SaveChangesAsync();
    }

    private async Task CheckForOverlappingAsync(Screening screening, int length)
    {
        var overlappingScreenings = await _context.Screenings
            .Where(s => s.RoomId == screening.RoomId
                        && s.StartsAt <= screening.StartsAt.AddMinutes(length)
                        && screening.StartsAt <= s.StartsAt.AddMinutes(s.Movie.Length))
            .ToListAsync();
        
        if(overlappingScreenings.Count != 0)
            throw new InvalidDataException($"The screening overlaps with {overlappingScreenings.Count} records.");
    }
}