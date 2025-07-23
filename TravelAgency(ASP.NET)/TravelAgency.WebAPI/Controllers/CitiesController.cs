using AutoMapper;
using ELTE.TravelAgency.DataAccess.Services;
using ELTE.TravelAgency.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace ELTE.TravelAgency.WebAPI.Controllers;

/// <summary>
/// Városok vezérlője
/// </summary>
[ApiController]
[Route("api/cities")] 
public class CitiesController : ControllerBase
{
    readonly ICityService _cityService;
    readonly IMapper _mapper;
    
    public CitiesController(ICityService cityService, IMapper mapper)
    {
        _cityService = cityService;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Városok listázása.
    /// </summary>
    /// <response code="200">Városok listája</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCities()
    {
        var cities = await _cityService.GetAllAsync();
        var cityDtos = _mapper.Map<List<CityDto>>(cities);

        return Ok(cityDtos);
    }
}