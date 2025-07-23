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
/// MoviesController
/// </summary>
[ApiController]
[Route("/movies")]
public class MoviesController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMoviesService _moviesService;
    private readonly IMoviesNotificationService _moviesNotificationService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="mapper"></param>
    /// <param name="moviesService"></param>
    /// <param name="moviesNotificationService"></param>
    public MoviesController(IMapper mapper, 
        IMoviesService moviesService,
        IMoviesNotificationService moviesNotificationService)
    {
        _mapper = mapper;
        _moviesService = moviesService;
        _moviesNotificationService = moviesNotificationService;
    }

    /// <summary>
    /// Add a new movie
    /// </summary>
    /// <param name="movieRequestDto"></param>
    /// <response code="201">Movie created successfully</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="409">Conflict</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(statusCode: StatusCodes.Status201Created, type: typeof(MovieResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateMovie([FromBody] MovieRequestDto movieRequestDto)
    {
        var movie = _mapper.Map<Movie>(movieRequestDto);
        await _moviesService.AddAsync(movie);

        var movieResponseDto = _mapper.Map<MovieResponseDto>(movie);
        
        var movieSignalRDto = _mapper.Map<MovieNotificationDto>(movie);
        await _moviesNotificationService.NotifyNewMovieAddedAsync(movieSignalRDto);

        return CreatedAtAction(nameof(CreateMovie), new { id = movieResponseDto.Id }, movieResponseDto);
    }

    /// <summary>
    /// Get a movie by ID
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">The requested movie</response>
    /// <response code="404">Not found</response>
    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(MovieResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMovieById([FromRoute] int id)
    {
        var movie = await _moviesService.GetByIdAsync(id);
        var movieResponseDto = _mapper.Map<MovieResponseDto>(movie);

        return Ok(movieResponseDto);
    }

    /// <summary>
    /// Get all movies in descending order by creation date
    /// </summary>
    /// <param name="count">An optional parameter to restrict the number of the returned movies</param>
    /// <response code="200">A list of movies</response>
    [HttpGet]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(List<MovieResponseDto>))]
    public async Task<IActionResult> GetMovies(int? count = null)
    {
        var movies = await _moviesService.GetLatestMoviesAsync(count);
        var movieResponseDtos = _mapper.Map<List<MovieResponseDto>>(movies);

        return Ok(movieResponseDtos);
    }
    
    /// <summary>
    /// Update a movie
    /// </summary>
    /// <param name="movieRequestDto"></param>
    /// <param name="id"></param>
    /// <response code="200">Movie updated successfully</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Not found</response>
    /// <response code="409">Conflict</response>
    [HttpPut]
    [Route("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(MovieResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateMovie([FromRoute] int id, [FromBody] MovieRequestDto movieRequestDto)
    {
        var movie = _mapper.Map<Movie>(movieRequestDto);
        movie.Id = id;

        await _moviesService.UpdateAsync(movie);

        var movieResponseDto = _mapper.Map<MovieResponseDto>(movie);

        return Ok(movieResponseDto);
    }

    /// <summary>
    /// Delete a movie
    /// </summary>
    /// <param name="id"></param>
    /// <response code="204">Movie deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden</response>
    /// <response code="404">Not found</response>
    [HttpDelete]
    [Route("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteMovie([FromRoute] int id)
    {
        await _moviesService.DeleteAsync(id);
        return NoContent();
    }
}
