using ELTE.Cinema.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace ELTE.Cinema.WebAPI.Controllers;

/// <summary>
/// E2ETestController
/// </summary>
[ApiController]
[Route("e2e-test")]
public class E2ETestController : ControllerBase
{
    private readonly DbResetter _dbResetter;
    private readonly IWebHostEnvironment _webHostEnvironment;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbResetter"></param>
    /// <param name="webHostEnvironment"></param>
    public E2ETestController(DbResetter dbResetter, IWebHostEnvironment webHostEnvironment)
    {
        _dbResetter = dbResetter;
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    /// Clears the database, then initializes it with test data
    /// </summary>
    /// <returns></returns>
    [HttpPost("reset-database")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> ResetDatabase()
    {
        if (!_webHostEnvironment.IsEnvironment("E2E"))
            return StatusCode(501);
        
        await _dbResetter.ResetAsync();
        return NoContent();
    }
}