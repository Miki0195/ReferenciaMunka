using ELTE.Cinema.DataAccess.Config;
using ELTE.Cinema.DataAccess.Exceptions;
using ELTE.Cinema.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ELTE.Cinema.DataAccess.Services;

internal class ReservationsService : IReservationsService
{
    private readonly CinemaDbContext _context;
    private readonly ReservationSettings _reservationSettings;
    private readonly IEmailsService _emailsService;
    private readonly IUsersService _usersService;

    public ReservationsService(
        CinemaDbContext context,
        IOptions<ReservationSettings> reservationSettings,
        IEmailsService emailsService,
        IUsersService usersService)
    {
        _context = context;
        _reservationSettings = reservationSettings.Value;
        _emailsService = emailsService;
        _usersService = usersService;
    }

    public async Task<List<Reservation>> GetAllReservationsAsync()
    {
        var reservations = _context.Reservations
            .Where(r => _usersService.IsCurrentUserAdmin() || r.UserId == _usersService.GetCurrentUserId());
        
        return await reservations.ToListAsync();
    }

    public async Task<Reservation> GetByIdAsync(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation is null)
            throw new EntityNotFoundException(nameof(Reservation));

        CheckUserReservation(reservation);

        return reservation;
    }

    public async Task AddAsync(long screeningId, Reservation reservation)
    {
        if (reservation.Seats.Count == 0)
            throw new ArgumentException("At least 1 seat must be selected.", nameof(reservation.Seats));
        if (reservation.Seats.Count > _reservationSettings.MaximumNumberOfSeats)
            throw new ArgumentException($"Positions contains more items than {_reservationSettings.MaximumNumberOfSeats}.", nameof(reservation.Seats));

        var duplicates = reservation.Seats.GroupBy(s => s.Position);
        if (duplicates.Any(g => g.Count() > 1))
            throw new InvalidDataException("Duplicate positions are not allowed.");

        var screening = await _context.Screenings
            .Include(s => s.Seats)
            .Include(s => s.Room)
            .Include(s => s.Movie)
            .FirstOrDefaultAsync(s => s.Id == screeningId);

        if (screening is null)
            throw new EntityNotFoundException(nameof(Screening));

        reservation.CreatedAt = DateTime.UtcNow;
        reservation.UserId = _usersService.GetCurrentUserId();

        foreach (var seat in reservation.Seats)
        {
            if (seat.ScreeningId != screeningId)
                throw new ArgumentException("All seats must belong to the same screening.", nameof(reservation.Seats));

            if (seat.Position.Row < 1 || screening.Room.Rows < seat.Position.Row)
                throw new ArgumentOutOfRangeException(nameof(seat.Position), $"Invalid position: {seat.Position.Row}, {seat.Position.Column}.");

            if (seat.Position.Column < 1 || screening.Room.Columns < seat.Position.Column)
                throw new ArgumentOutOfRangeException(nameof(seat.Position), $"Invalid position: {seat.Position.Row}, {seat.Position.Column}.");

            var alreadyReserved = screening.Seats.Any(s => s.Position == seat.Position);
            if (alreadyReserved)
                throw new ArgumentException($"Position: {seat.Position.Row}, {seat.Position.Column} is already reserved or sold.", nameof(seat.Position));
        }

        _context.Reservations.Add(reservation);

        try
        {
            await _context.SaveChangesAsync();

            // Send email notification, but do not wait for it for better performance
            // (Could be improved with error logging though)
            _ = _emailsService.SendEmailAsync(reservation.Email,
                "[ELTE Cinema] New reservation",
                GenerateEmailBody(screening, reservation));
        }
        catch (DbUpdateException ex)
        {
            throw new SaveFailedException("Failed to create reservation", ex);
        }
    }

    public async Task CancelAsync(int id)
    {
        var reservation = await GetByIdAsync(id);

        CheckUserReservation(reservation);

        if (reservation.Seats.First().Screening.StartsAt < DateTime.Now)
        {
            throw new InvalidDataException("Cannot cancel reservation for a past screening date.");
        }

        try
        {
            // The seats will be also deleted. See CinemaDbContext.OnModelCreating for the cascading configuration.
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new SaveFailedException("Failed to cancel reservation", ex);
        }
    }

    private static string GenerateEmailBody(Screening screening, Reservation reservation)
    {
        return $"""
<p>
    <b>Your reservation has been created successfully!</b>
</p>
<table>
    <tr>
        <td><b>Movie:</b></td>
        <td>{screening.Movie.Title}</td>
    </tr>
    <tr>
        <td><b>Time:</b></td>
        <td>{screening.StartsAt}</td>
    </tr>
    <tr>
        <td><b>Room:</b></td>
        <td>{screening.Room.Name}</td>
    </tr>
    <tr>
        <td><b>Seats:</b></td>
        <td>{string.Join(", ", reservation.Seats.Select(s => $"(Row {s.Position.Row}, Col {s.Position.Column})"))}</td>
    </tr>
</table>
""";
    }
    private void CheckUserReservation(Reservation reservation)
    {
        if (!_usersService.IsCurrentUserAdmin() && reservation.UserId != _usersService.GetCurrentUserId())
            throw new AccessViolationException("Reservation is not accessible");
    }
}