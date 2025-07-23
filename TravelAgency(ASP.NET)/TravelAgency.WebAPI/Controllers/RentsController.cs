using AutoMapper;
using ELTE.TravelAgency.DataAccess.Models;
using ELTE.TravelAgency.DataAccess.Services;
using ELTE.TravelAgency.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELTE.TravelAgency.WebAPI.Controllers;

/// <summary>
/// Foglalások vezérlője
/// </summary>
[ApiController]
[Route("api/rents")] 
public class RentsController : ControllerBase
{
    private readonly IRentService _rentService;
    private readonly IApartmentService _apartmentService;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    public RentsController(IRentService rentService, IApartmentService apartmentService, IUserService userService, IMapper mapper)
    {
        _rentService = rentService;
        _apartmentService = apartmentService;
        _userService = userService;
        _mapper = mapper;
    }

    /// <summary>
    /// Foglalások listázása.
    /// </summary>
    /// <param name="apartmentId">Apartment azonosítója</param>
    /// <response code="200">Foglalások listája</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<RentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRents([FromQuery] int? apartmentId)
    {
        var rents = (await _rentService.GetAllAsync(apartmentId)).ToList();
            rents.ForEach(r =>
            {
                r.Email = string.Empty;
                r.PhoneNumber = string.Empty;
                r.Address = string.Empty;
            });
            
        var rentDtos = _mapper.Map<List<RentDto>>(rents);
        return Ok(rentDtos);
    }
    
    /// <summary>
    /// Új foglalás létrehozása
    /// </summary>
    /// <response code="200">Sikeres foglalás</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(RentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest)]
    [ProducesResponseType(statusCode: StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRent([FromBody] RentDto rentDto)
    {
        try
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (!string.Equals(currentUser!.Email, rentDto.Email, StringComparison.InvariantCultureIgnoreCase))
                return Conflict("Érvénytelen email cím");
            
            var error = await _apartmentService.IsAvailable(rentDto.StartDate, rentDto.EndDate, rentDto.ApartmentId);
            if (error > RentDateError.None)
                return HandleError(error);
            
            rentDto.Id = null;

            var rent = _mapper.Map<Rent>(rentDto);
            await _rentService.AddAsync(rent);
        
            var rentResponse = _mapper.Map<RentDto>(rent);

            return Ok(rentResponse);
        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    private IActionResult HandleError(RentDateError error)
    {
        return error switch
        {
            RentDateError.StartInvalid => BadRequest("A kezdés dátuma nem megfelelő (túl korai, vagy nem fordulónapra esik)!"),
            RentDateError.EndInvalid => BadRequest("A megadott foglalási idő érvénytelen (a foglalás vége korábban van, mint a kezdete)!"),
            RentDateError.LengthInvalid => BadRequest("A megadott foglalási idő érvénytelen (egész heteket lehet csak foglalni)!"),
            RentDateError.Conflicting => Conflict("A megadott időpontban a szállás már foglalt!"),
            RentDateError.ApartmentNotExists => NotFound("A megadott apartmant nem létezik!"),
            _ => BadRequest("Hiba történt")
        };
    }
}