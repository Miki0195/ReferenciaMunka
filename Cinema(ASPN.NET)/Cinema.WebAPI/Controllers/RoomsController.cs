using AutoMapper;
using ELTE.Cinema.DataAccess.Models;
using ELTE.Cinema.DataAccess.Services;
using ELTE.Cinema.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELTE.Cinema.WebAPI.Controllers;

/// <summary>
/// RoomsController
/// </summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IRoomsService _roomsService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mapper"></param>
    /// <param name="roomsService"></param>
    public RoomsController(IMapper mapper, IRoomsService roomsService)
    {
        _mapper = mapper;
        _roomsService = roomsService;
    }

    /// <summary>
    /// Add a new room
    /// </summary>
    /// <param name="roomRequestDto"></param>
    /// <response code="201">Room created successfully</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="409">Conflict</response>
    [HttpPost]
    [ProducesResponseType(statusCode: StatusCodes.Status201Created, type: typeof(RoomResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRoom([FromBody] RoomRequestDto roomRequestDto)
    {
        var room = _mapper.Map<Room>(roomRequestDto);
        await _roomsService.AddAsync(room);

        var roomResponseDto = _mapper.Map<RoomResponseDto>(room);
        return CreatedAtAction(nameof(CreateRoom), new { id = roomResponseDto.Id }, roomResponseDto);
    }

    /// <summary>
    /// Get all rooms
    /// </summary>
    /// <response code="200">A list of rooms</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<RoomResponseDto>))]
    public async Task<IActionResult> GetRooms()
    {
        var rooms = await _roomsService.GetAllAsync();
        var roomResponseDtos = _mapper.Map<List<RoomResponseDto>>(rooms);

        return Ok(roomResponseDtos);
    }
    
    /// <summary>
    /// Get a room by ID
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">The requested room</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Not found</response>
    [HttpGet]
    [AllowAnonymous]
    [Route("{id}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(RoomResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoomById([FromRoute] long id)
    {
        var room = await _roomsService.GetByIdAsync((int)id);
        var roomResponseDto = _mapper.Map<RoomResponseDto>(room);

        return Ok(roomResponseDto);
    }
    
    /// <summary>
    /// Update a room
    /// </summary>
    /// <param name="roomRequestDto"></param>
    /// <param name="id"></param>
    /// <response code="200">Room updated successfully</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Not found</response>
    [HttpPut]
    [Route("{id}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(RoomResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateRoom([FromRoute] long id, [FromBody] RoomRequestDto roomRequestDto)
    {
        var room = _mapper.Map<Room>(roomRequestDto);
        room.Id = (int)id;

        await _roomsService.UpdateAsync(room);

        var roomResponseDto = _mapper.Map<RoomResponseDto>(room);

        return Ok(roomResponseDto);
    }

    /// <summary>
    /// Delete a room
    /// </summary>
    /// <param name="id"></param>
    /// <response code="204">Room deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Not found</response>
    [HttpDelete]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRoom([FromRoute] long id)
    {
        await _roomsService.DeleteAsync((int)id);
        return NoContent();
    }
    
}
