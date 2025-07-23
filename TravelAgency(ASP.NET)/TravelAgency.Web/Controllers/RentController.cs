using ELTE.TravelAgency.DataAccess.Models;
using ELTE.TravelAgency.Web.Models;
using System;
using System.Threading.Tasks;
using ELTE.TravelAgency.DataAccess.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace ELTE.TravelAgency.Web.Controllers
{
    /// <summary>
    /// Foglalások vezérlője.
    /// </summary>
    public class RentController : BaseController
    {
        private readonly IApartmentService _apartmentService;
        private readonly IRentService _rentService;

        /// <summary>
        /// Vezérlő példányosítása.
        /// </summary>
        public RentController(
            ICityService cityService,
            IApartmentService apartmentService,
            IRentService rentService,
            IMapper mapper)
            : base(cityService, mapper)
        {
            _apartmentService = apartmentService;
            _rentService = rentService;
        }

        /// <summary>
        /// Foglalás (oldal lekérése).
        /// </summary>
        /// <param name="apartmentId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index(int apartmentId)
        {
            try
            {
                var apartment = await _apartmentService.GetAsync(apartmentId);
                var apartmentViewModel = _mapper.Map<ApartmentViewModel>(apartment);

                RentViewModel rentViewModel = new RentViewModel
                {
                    Apartment = apartmentViewModel,
                    Name = string.Empty,
                    Email = string.Empty,
                    Address = string.Empty,
                    PhoneNumber = string.Empty
                }; // létrehozunk egy új foglalást, amelynek megadjuk az apartmant

                // beállítunk egy foglalást, amely a következő megfelelő fordulónappal (minimum 1 héttel később), és egy hetes időtartammal rendelkezik
                rentViewModel.StartDate = DateTime.Today + TimeSpan.FromDays(7);
                while (rentViewModel.StartDate.DayOfWeek != apartment.Turnday)
                    rentViewModel.StartDate += TimeSpan.FromDays(1);

                rentViewModel.EndDate = rentViewModel.StartDate + TimeSpan.FromDays(7);

                return View("Index", rentViewModel);
            }
            catch
            {
                // ha nem sikerül (nem volt jó az azonosító)
                return RedirectToAction("Index", "Home"); // visszairányítjuk a főoldalra
            }
        }

        /// <summary>
        /// Foglalás (adatok beküldése).
        /// </summary>
        /// <param name="apartmentId">Apartman azonosítója.</param>
        /// <param name="rentViewModel">Foglalás adatai.</param>
        /// <returns>Foglalás eredmény nézete.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken] // védelem XSRF támadás ellen
        public async Task<IActionResult> Index(int apartmentId, RentViewModel rentViewModel)
        {
            try
            {
                var apartment = await _apartmentService.GetAsync(apartmentId);
                var apartmentViewModel = _mapper.Map<ApartmentViewModel>(apartment);

                rentViewModel.Apartment = apartmentViewModel;
                ModelState.Clear(); // töröljük az Apartman hiányából eredő validációs hibát
                TryValidateModel(rentViewModel); // újra validáljuk a modellt
            }
            catch
            {
                // nem találjuk az apartmant
                return RedirectToAction("Index", "Home");
            }

            switch (await _apartmentService.IsAvailable(rentViewModel.StartDate, rentViewModel.EndDate, apartmentId))
            {
                case RentDateError.StartInvalid:
                    ModelState.AddModelError("StartDate",
                        "A kezdés dátuma nem megfelelő (túl korai, vagy nem fordulónapra esik)!");
                    break;
                case RentDateError.EndInvalid:
                    ModelState.AddModelError("EndDate",
                        "A megadott foglalási idő érvénytelen (a foglalás vége korábban van, mint a kezdete)!");
                    break;
                case RentDateError.LengthInvalid:
                    ModelState.AddModelError("EndDate",
                        "A megadott foglalási idő érvénytelen (egész heteket lehet csak foglalni)!");
                    break;
                case RentDateError.Conflicting:
                    ModelState.AddModelError("StartDate", "A megadott időpontban a szállás már foglalt!");
                    break;
                // az apartman biztosan létezik ezen a ponton, így azt nem kezeljük
            }

            if (!ModelState.IsValid)
                return View("Index", rentViewModel);

            try
            {
                Rent rent = new Rent
                {
                    ApartmentId = rentViewModel.Apartment.Id,
                    StartDate = rentViewModel.StartDate,
                    EndDate = rentViewModel.EndDate,
                    Name = rentViewModel.Name,
                    Email = rentViewModel.Email,
                    Address = rentViewModel.Address,
                    PhoneNumber = rentViewModel.PhoneNumber
                };

                await _rentService.AddAsync(rent);
            }
            catch(DbUpdateException ex)
            {
                ModelState.AddModelError("", "A foglalás rögzítése sikertelen, kérem próbálja újra!");
                return View("Index", rentViewModel);
            }

            // kiszámoljuk a teljes árat
            rentViewModel.TotalPrice = await _apartmentService.GetPrice(
                rentViewModel.StartDate, rentViewModel.EndDate,
                apartmentId);

            ViewBag.Message = "A foglalását sikeresen rögzítettük!";
            return View("Result", rentViewModel);
        }
    }
}