using ELTE.TravelAgency.DataAccess.Models;
using ELTE.TravelAgency.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ELTE.TravelAgency.DataAccess.Services;

namespace ELTE.TravelAgency.Web.Controllers
{
	/// <summary>
	/// Vezérlő típusa
	/// </summary>
	public class HomeController : BaseController
	{
        private readonly IBuildingService _buildingService;

        // Google konfiguráció
        private readonly IOptions<GoogleConfig> _googleConfig;

        /// <summary>
        /// Vezérlő példányosítása.
        /// </summary>
        public HomeController(
            ICityService cityService,
            IBuildingService buildingService,
            IMapper mapper,
            IOptions<GoogleConfig> googleConfig)
            : base(cityService, mapper)

        {
            _buildingService = buildingService;
            _googleConfig = googleConfig;
        }

        /// <summary>
		/// Épületek listázása.
		/// </summary>
		/// <returns>Az épületek listájának nézete.</returns>
		public async Task<IActionResult> Index()
        {
            var buildings = await _buildingService.GetAllAsync();
			var buildingViewModels = _mapper.Map<List<BuildingViewModel>>(buildings);

            return View("Index", buildingViewModels);
		}

		/// <summary>
		/// Épületek listázása.
		/// </summary>
		/// <param name="cityId">Város azonosítója.</param>
		/// <returns>Az épületek listájának nézete.</returns>
		public async Task<IActionResult> List(int cityId)
		{
			// minden lekérdezés a modellen keresztül történik
            var buildings = await _buildingService.GetByCityAsync(cityId);

            if (!buildings.Any()) // ha nincs ilyen épület
				return RedirectToAction(nameof(Index)); // átirányítjuk a kezdőoldalra

            var buildingViewModels = _mapper.Map<List<BuildingViewModel>>(buildings);
            return View("Index", buildingViewModels);
		}

		/// <summary>
		/// Épület részleteinek nézete.
		/// </summary>
		/// <param name="buildingId">Épület azonosítója.</param>
		/// <returns>Az épület részletes nézete.</returns>
		public async Task<IActionResult> Details(int buildingId)
		{
            try
            {
                Building building = await _buildingService.GetWithApartments(buildingId);

                // az oldal címe
                ViewBag.Title = "Épület részletei: " + building.Name + " (" + building.City.Name + ")";
                // Google Maps API Key
                ViewBag.GoogleMapsApiKey = _googleConfig.Value.MapsApiKey;

                var buildingViewModel = _mapper.Map<BuildingViewModel>(building);
                var buildingDetailsViewModel = _mapper.Map<BuildingDetailsViewModel>(buildingViewModel);
                buildingDetailsViewModel.Images = await _buildingService.GetImageIdsAsync(building.Id);
                // az épülethez tartozó képek azonosítói

                return View("Details", buildingDetailsViewModel);
            }
            catch
            {
                // nem találjuk az épületet
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
		/// Épület főképének lekérdezése.
		/// </summary>
		/// <param name="buildingId">Épület azonosítója.</param>
		/// <returns>Az épület képe, vagy az alapértelmezett kép.</returns>
		public async Task<FileResult> ImageForBuilding(int buildingId)
        {
            // lekérjük az épület első tárolt képét (kicsiben)
            byte[]? imageContent = await _buildingService.GetMainImageAsync(buildingId);

            if (imageContent == null) // amennyiben nem sikerült betölteni, egy alapértelmezett képet adunk vissza
                return File("~/images/NoImage.png", "image/png");

            return File(imageContent, "image/png");
        }

        /// <summary>
        /// Épület egyik képének lekérdezése.
        /// </summary>
        /// <param name="imageId">Kép azonosítója.</param>
        /// <param name="large">Nagy méretű kép lekérése.</param>
        /// <returns>Az épület egy képe, vagy az alapértelmezett kép.</returns>
        public async Task<FileResult> Image(int imageId, Boolean large = false)
        {
            try
            {
                // lekérjük a megadott azonosítóval rendelkező képet
                byte[]? imageContent = await _buildingService.GetImageAsync(imageId, large);

                return File(imageContent, "image/png");
            }
            catch
            {
                // amennyiben nem sikerült betölteni, egy alapértelmezett képet adunk vissza
                return File("~/images/NoImage.png", "image/png");
            }
        }
    }
}
