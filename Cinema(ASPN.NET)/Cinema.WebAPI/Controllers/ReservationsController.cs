using AutoMapper;
using ELTE.Cinema.DataAccess.Models;
using ELTE.Cinema.DataAccess.Services;
using ELTE.Cinema.Shared.Models;
using ELTE.Cinema.Shared.SignalR.Models;
using ELTE.Cinema.SignalR.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELTE.Cinema.WebAPI.Controllers;

/// <summary>
/// ReservationsController
/// </summary>
[ApiController]
[Route("/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IReservationsService _reservationsService;
    private readonly IScreeningsNotificationService? _screeningsNotificationService;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mapper"></param>
    /// <param name="reservationsService"></param>
    /// <param name="screeningNotificationService"></param>
    public ReservationsController(IMapper mapper, IReservationsService reservationsService, IScreeningsNotificationService? screeningNotificationService = null)
    {
        _mapper = mapper;
        _reservationsService = reservationsService;
        _screeningsNotificationService = screeningNotificationService;

    }

    /// <summary>
    /// Add a new reservation
    /// </summary>
    /// <param name="reservationRequestDto"></param>
    /// <response code="201">Reservation created successfully</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="409">Conflict</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(statusCode: StatusCodes.Status201Created, type: typeof(ReservationResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReservation([FromBody] ReservationRequestDto reservationRequestDto)
    {
        var reservation = _mapper.Map<Reservation>(reservationRequestDto);
        await _reservationsService.AddAsync(reservationRequestDto.ScreeningId, reservation);

        var reservationResponseDto = _mapper.Map<ReservationResponseDto>(reservation);
        
        var seatSignalRDtos = _mapper.Map<List<SeatNotificationDto>>(reservation);
        _screeningsNotificationService?.NotifySeatsStatusChangedAsync(reservationResponseDto.Screening.Id, seatSignalRDtos);
        
        return CreatedAtAction(nameof(CreateReservation), new { id = reservationResponseDto.Id }, reservationResponseDto);
    }

    /// <summary>
    /// Delete a reservation
    /// </summary>
    /// <param name="id"></param>
    /// <response code="204">Reservation deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">NotFound</response>
    /// <response code="409">Conflict</response>
    [HttpDelete]
    [Route("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteReservation([FromRoute] int id)
    {
        var reservation = await _reservationsService.GetByIdAsync(id);
        var screeningId = reservation.Seats.First().ScreeningId;
        
        var seatSignalRDtos = _mapper.Map<List<SeatNotificationDto>>(reservation);

        foreach (var seat in seatSignalRDtos)
        {
            seat.Status = SeatClientStatusDto.None;
        }
        
        await _reservationsService.CancelAsync(id);
        _screeningsNotificationService?.NotifySeatsStatusChangedAsync(screeningId, seatSignalRDtos);
        
        return NoContent();
    }

    /// <summary>
    /// Get a reservation by ID
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">The requested reservation</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">NotFound</response>
    [HttpGet]
    [Route("/reservations/{id}")]
    [Authorize]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ReservationResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservationById([FromRoute] int id)
    {
        var reservation = await _reservationsService.GetByIdAsync(id);
        var reservationResponseDto = _mapper.Map<ReservationResponseDto>(reservation);

        return Ok(reservationResponseDto);
    }

    /// <summary>
    /// Get all reservations
    /// </summary>
    /// <response code="200">A list of reservations</response>
    /// <response code="401">Unauthorized</response>
    [HttpGet]
    [Route("/reservations")]
    [Authorize]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<ReservationResponseDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReservations()
    {
        var reservations = await _reservationsService.GetAllReservationsAsync();
        var reservationResponseDtOs = _mapper.Map<List<ReservationResponseDto>>(reservations);

        return Ok(reservationResponseDtOs);
    }
}
