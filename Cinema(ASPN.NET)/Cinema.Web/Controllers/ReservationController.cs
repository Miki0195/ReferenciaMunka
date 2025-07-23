using AutoMapper;
using ELTE.Cinema.DataAccess.Config;
using ELTE.Cinema.DataAccess.Exceptions;
using ELTE.Cinema.DataAccess.Models;
using ELTE.Cinema.DataAccess.Services;
using ELTE.Cinema.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ELTE.Cinema.Web.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IScreeningsService _screeningsService;
        private readonly IReservationsService _reservationsService;
        private readonly IMapper _mapper;
        private readonly ReservationSettings _reservationSettings;

        public ReservationController(
            IScreeningsService screeningsService,
            IMapper mapper,
            IReservationsService reservationsService,
            IOptions<ReservationSettings> reservationSettings
        )
        {
            _mapper = mapper;
            _screeningsService = screeningsService;
            _reservationsService = reservationsService;
            _reservationSettings = reservationSettings.Value;
        }

        public async Task<IActionResult> Index(int screeningId)
        {
            try
            {
                var screening = await _screeningsService.GetByIdAsync(screeningId);
                var screeningViewModel = _mapper.Map<ScreeningViewModel>(screening);

                int rows = screening.Room.Rows;
                int columns = screening.Room.Columns;

                var seats = new List<SeatViewModel>();
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        seats.Add(new SeatViewModel
                        {
                            Row = i,
                            Column = j,
                            Status = screening.Seats.Any(s => s.Position.Column == j && s.Position.Row == i)
                                ? SeatViewModelStatus.Reserved
                                : SeatViewModelStatus.Free
                        });
                    }
                }

                return View(new ReservationPageViewModel()
                {
                    ScreeningViewModel = screeningViewModel,
                    SeatViewModels = seats,
                    MaximumNumberOfSeats = _reservationSettings.MaximumNumberOfSeats
                });
            }
            catch (EntityNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reserve([FromBody] ReservationViewModel reservationData)
        {
            Reservation reservation = _mapper.Map<Reservation>(reservationData);
            await _reservationsService.AddAsync(reservationData.ScreeningId, reservation);
            return new OkResult();
        }
    }
}
