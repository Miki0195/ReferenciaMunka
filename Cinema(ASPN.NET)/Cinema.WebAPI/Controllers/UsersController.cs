using System.ComponentModel.DataAnnotations;
using AutoMapper;
using ELTE.Cinema.DataAccess.Models;
using ELTE.Cinema.DataAccess.Services;
using ELTE.Cinema.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELTE.Cinema.WebAPI.Controllers;

/// <summary>
/// UsersController
/// </summary>
[ApiController]
[Route("/users")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly IMapper _mapper;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mapper"></param>
    /// <param name="usersService"></param>
    public UsersController(IMapper mapper, IUsersService usersService)
    {
        _mapper = mapper;
        _usersService = usersService;
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="userRequestDto"></param>
    /// <response code="201">User created successfully</response>
    /// <response code="400">Bad Request</response>
    /// <response code="409">Conflict</response>
    [HttpPost]
    [ProducesResponseType(statusCode: StatusCodes.Status201Created, type: typeof(UserResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] UserRequestDto userRequestDto)
    {
        var user = _mapper.Map<User>(userRequestDto);

        await _usersService.AddUserAsync(user, userRequestDto.Password);

        var userResponseDto = _mapper.Map<UserResponseDto>(user);

        return StatusCode(StatusCodes.Status201Created, userResponseDto);
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">User</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Not found</response>
    [HttpGet]
    [Route("{id}")]
    [Authorize]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(UserResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser([FromRoute][Required] string id)
    {
        var user = await _usersService.GetUserByIdAsync(id);
        var userResponseDto = _mapper.Map<UserResponseDto>(user);

        return Ok(userResponseDto);
    }

    /// <summary>
    /// Login
    /// </summary>
    /// <param name="loginRequestDto"></param>
    /// <response code="200">Login was successful</response>
    /// <response code="400">Bad Request</response>
    /// <response code="403">Forbidden</response>
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(UserResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
    {
        var (authToken, refreshToken, userId) = await _usersService.LoginAsync(loginRequestDto.Email, loginRequestDto.Password);

        var loginResponseDto = new LoginResponseDto
        {
            UserId = userId,
            AuthToken = authToken,
            RefreshToken = refreshToken,
        };

        return Ok(loginResponseDto);
    }

    /// <summary>
    /// Logout
    /// </summary>
    /// <response code="200">Logout was successful</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost]
    [Route("logout")]
    [Authorize]
    [ProducesResponseType(statusCode: StatusCodes.Status204NoContent, type: typeof(void))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        await _usersService.LogoutAsync();

        return NoContent();
    }

    /// <summary>
    /// Redeem refresh token
    /// </summary>
    /// <response code="200">Login was successful</response>
    [HttpPost]
    [Route("refresh")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(UserResponseDto))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RedeemRefreshToken([FromBody] string refreshToken)
    {
        var (authToken, newRefreshToken, userId) = await _usersService.RedeemRefreshTokenAsync(refreshToken);

        var loginResponseDto = new LoginResponseDto
        {
            UserId = userId,
            AuthToken = authToken,
            RefreshToken = newRefreshToken,
        };

        return Ok(loginResponseDto);
    }
}
