using AutoMapper;
using ELTE.TravelAgency.DataAccess.Models;
using ELTE.TravelAgency.DataAccess.Services;
using ELTE.TravelAgency.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELTE.TravelAgency.WebAPI.Controllers;

/// <summary>
///  Épületek vezérlője
/// </summary>
[ApiController]
[Route("api/buildings")]
public class BuildingsController : ControllerBase
{
    private readonly IBuildingService _buildingService;
    private readonly IMapper _mapper;

    public BuildingsController(IBuildingService buildingService, IMapper mapper)
    {
        _buildingService = buildingService;
        _mapper = mapper;
    }

    /// <summary>
    /// Épületek listázása.
    /// </summary>
    /// <response code="200">Az épületek listája</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<BuildingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBuildings(int? cityId)
    {
        var buildings = cityId.HasValue
            ? await _buildingService.GetByCityAsync(cityId!.Value)
            : await _buildingService.GetAllAsync();
        var buildingDtos = _mapper.Map<List<BuildingDto>>(buildings);

        return Ok(buildingDtos);
    }

    /// <summary>
    /// Épület részleteinek nézete.
    /// </summary>
    /// <param name="id">Épület azonosítója</param>
    /// <response code="200">Épület részletei</response>
    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(BuildingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBuildingById(int id)
    {
        var building = await _buildingService.GetWithApartments(id);
        var buildingDto = _mapper.Map<BuildingDto>(building);

        return Ok(buildingDto);
    }

    /// <summary>
    /// Épület főképének lekérdezése.
    /// </summary>
    /// <param name="id"></param>
    /// <response code="200">Épület képei</response>
    [HttpGet]
    [Route("{id}/images")]
    [ProducesResponseType(typeof(List<ImageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImagesForBuilding(int id)
    {
        var images = await _buildingService.GetImagesAsync(id);
        var imageDtos = _mapper.Map<List<ImageDto>>(images);

        return Ok(imageDtos);
    }

    /// <summary>
    /// Épület főképének lekérdezése.
    /// </summary>
    /// <param name="id">Épület azonosítója.</param>
    /// <param name="large">Nagy méretű kép változat?</param>
    /// <response code="200">Az épület képe, vagy az alapértelmezett kép.</response>
    [HttpGet]
    [Route("{id}/images/main")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<FileResult> ImageForBuilding(int id, [FromQuery] bool large = false)
    {
        var imageContent = await _buildingService.GetMainImageAsync(id, large);

        if (imageContent == null) // amennyiben nem sikerült betölteni, egy alapértelmezett képet adunk vissza
            return File("~/images/NoImage.png", "image/png");

        return File(imageContent, "image/png");
    }

    /// <summary>
    /// Új épület létrehozása.
    /// </summary>
    /// <param name="buildingDto">Épület.</param>
    [HttpPost]
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> PostBuilding([FromBody] BuildingDto buildingDto)
    {
        var building = _mapper.Map<Building>(buildingDto);
        building.Id = 0;

        try
        {
            await _buildingService.AddAsync(building);
            buildingDto.Id = building.Id;

            // visszaküldjük a létrehozott épületet
            return CreatedAtAction(nameof(GetBuildingById), new { id = building.Id }, buildingDto);
        }
        catch
        {
            // Internal Server Error
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Épület módosítása.
    /// </summary>
    /// <param name="buildingDto">Épület.</param>
    [HttpPut]
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> PutBuilding([FromBody] BuildingDto buildingDto)
    {
        var building = _mapper.Map<Building>(buildingDto);

        try
        {
            await _buildingService.UpdateAsync(building);
            return Ok();
        }
        catch (ArgumentException) // ha nincs ilyen azonosító, akkor hibajelzést küldünk
        {
            // Not Found
            return NotFound();
        }
        catch
        {
            // Internal Server Error
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Épület törlése.
    /// </summary>
    /// <param name="id">Épület azonosító.</param>
    [HttpDelete("{id}")]
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> DeleteBuilding(Int32 id)
    {
        try
        {
            await _buildingService.DeleteAsync(id);
            return Ok();
        }
        catch (InvalidOperationException) // ha nincs ilyen azonosító, akkor hibajelzést küldünk
        {
            // Not Found
            return NotFound();
        }
        catch
        {
            // Internal Server Error
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Kép feltöltése.
    /// </summary>
    /// <param name="imageDto">Kép.</param>
    [HttpPost("images")] // itt nem kell paramétereznünk, csak jelezzük, hogy az egyedi útvonalat vesszük igénybe
    [Authorize(Roles = "administrator")] // csak bejelentkezett adminisztrátoroknak
    public async Task<IActionResult> PostImage([FromBody] ImageDto imageDto)
    {
        var buildingImage = _mapper.Map<BuildingImage>(imageDto);
        buildingImage.Id = 0;

        try
        {
            await _buildingService.AddImageAsync(buildingImage);
            imageDto.Id = buildingImage.Id;

            return CreatedAtAction(nameof(ImageForBuilding), new { id = buildingImage.Id, large = true }, buildingImage.Id); // csak az azonosítót küldjük vissza
        }
        catch
        {
            // Internal Server Error
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Kép törlése.
    /// </summary>
    /// <param name="imageId">A kép azonosítója.</param>
    [HttpDelete("images/{imageId}")]
    [Authorize(Roles = "administrator")]
    public async Task<IActionResult> DeleteImage(int imageId)
    {
        try
        {
            await _buildingService.DeleteImageAsync(imageId);
            return Ok();
        }
        catch (InvalidOperationException) // ha nincs ilyen azonosító, akkor hibajelzést küldünk
        {
            // Not Found
            return NotFound();
        }
        catch
        {
            // Internal Server Error
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}