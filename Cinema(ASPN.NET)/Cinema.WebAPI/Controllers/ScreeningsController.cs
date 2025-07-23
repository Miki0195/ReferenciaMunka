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
/// ScreeningsController
/// </summary>
[ApiController]
[Route("/screenings")]
public class ScreeningsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IScreeningsService _screeningsService;
    private readonly IScreeningsNotificationService? _screeningsNotificationService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mapper"></param>
    /// <param name="screeningsService"></param>
    /// <param name="screeningsNotificationService"></param>
    public ScreeningsController(IMapper mapper, IScreeningsService screeningsService, IScreeningsNotificationService? screeningsNotificationService = null)
    {
        _mapper = mapper;
        _screeningsService = screeningsService;
        _screeningsNotificationService = screeningsNotificationService;
    }

    /// <summary>
    /// Add a new screening
    /// </summary>
    /// <param name="screeningRequestDto"></param>
    /// <response code="201">Screening created successfully</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="409">Conflict</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(statusCode: StatusCodes.Status201Created, type: typeof(ScreeningResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateScreening([FromBody] ScreeningRequestDto screeningRequestDto)
    {
        var screening = _mapper.Map<Screening>(screeningRequestDto);
        await _screeningsService.AddAsync(screening);

        var screeningResponseDto = _mapper.Map<ScreeningResponseDto>(screening);
        return CreatedAtAction(nameof(CreateScreening), new { id = screeningResponseDto.Id }, screeningResponseDto);
    }

    /// <summary>
    /// Get a screening by ID
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">The requested screening</response>
    /// <response code="404">Not found</response>
    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ScreeningResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetScreeningById([FromRoute] long id)
    {
        var screening = await _screeningsService.GetByIdAsync((int)id);
        var screeningResponseDto = _mapper.Map<ScreeningResponseDto>(screening);

        return Ok(screeningResponseDto);

    }

    /// <summary>
    /// Get all screenings
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="movieId"></param>
    /// <param name="roomId"></param>
    /// <param name="startsAfter"></param>
    /// <param name="startsBefore"></param>
    /// <response code="200">A list of screenings</response>
    /// <response code="400">Bad Request</response>
    [HttpGet]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<ScreeningResponseDto>))]
    public async Task<IActionResult> GetScreenings([FromQuery] int? page, [FromQuery] int? size, [FromQuery] int? movieId, [FromQuery] int? roomId, [FromQuery] DateTime? startsAfter, [FromQuery] DateTime? startsBefore)
    {
        var pagedResult = await _screeningsService.GetAllAsync(movieId, roomId, page, size, startsAfter, startsBefore);
        var screeningResponseDtos = _mapper.Map<List<ScreeningResponseDto>>(pagedResult.Items);

        // Nullable check required because of the Tests
        Response?.Headers.TryAdd("X-Count", pagedResult.Total.ToString());
        Response?.Headers.TryAdd("Access-Control-Expose-Headers", "X-Count");

        return Ok(screeningResponseDtos);
    }

    /// <summary>
    /// Update a screening
    /// </summary>
    /// <param name="screeningRequestDto"></param>
    /// <param name="id"></param>
    /// <response code="200">Screening updated successfully</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Not found</response>
    /// <response code="409">Conflict</response>
    [HttpPut]
    [Route("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ScreeningResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateScreening([FromRoute] long id, [FromBody] ScreeningRequestDto screeningRequestDto)
    {
        var screening = _mapper.Map<Screening>(screeningRequestDto);
        screening.Id = (int)id;
        await _screeningsService.UpdateAsync(screening);
        
        var screeningDto = _mapper.Map<ScreeningResponseDto>(screening);
        return Ok(screeningDto);
    }

    /// <summary>
    /// Get all reserved/sold seats for the given screening
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">A list of seats</response>
    /// <response code="404">Not found</response>
    [HttpGet]
    [Route("{id}/seats")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<SeatResponseDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSeatsByScreening([FromRoute] int id)
    {
        var seats = await _screeningsService.GetSeatsByScreeningAsync(id);
        var seatResponseDtos = _mapper.Map<List<SeatResponseDto>>(seats);

        return Ok(seatResponseDtos);
    }
    
    /// <summary>
    /// Delete a screening
    /// </summary>
    /// <param name="id"></param>
    /// <response code="204">Screening deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Not found</response>
    [HttpDelete]
    [Route("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteScreening([FromRoute] long id)
    {
        await _screeningsService.DeleteAsync((int)id);
        return NoContent();
    }

    /// <summary>
    /// Sell seat
    /// </summary>
    /// <param name="id"></param>
    /// <param name="seatRequestDto"></param>
    /// <response code="200">Screening updated successfully</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Not found</response>
    /// <response code="409">Conflict</response>
    [HttpPut]
    [Route("{id}/seats/sell")]
    [Authorize]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(SeatResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SellSeat([FromRoute] int id, [FromBody] SeatRequestDto seatRequestDto)
    {
        var seat = _mapper.Map<Seat>(seatRequestDto);
        await _screeningsService.SellSeatForScreeningAsync(id, seat);
        var seatResponseDto = _mapper.Map<SeatResponseDto>(seat);
        
        var seatSignalRDto = _mapper.Map<SeatNotificationDto>(seat);
        _screeningsNotificationService?.NotifySeatStatusChangedAsync(id, seatSignalRDto);
        
        return Ok(seatResponseDto);
    }
}
