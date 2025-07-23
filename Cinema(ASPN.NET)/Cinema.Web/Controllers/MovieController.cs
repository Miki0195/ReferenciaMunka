using AutoMapper;
using ELTE.Cinema.DataAccess.Models;
using ELTE.Cinema.DataAccess.Services;
using ELTE.Cinema.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ELTE.Cinema.Web.Controllers
{
    public class MovieController : Controller
    {
        private readonly IMoviesService _moviesService;
        private readonly IScreeningsService _screeningsService;
        private readonly IMapper _mapper;

        public MovieController(IMoviesService moviesService, IScreeningsService screeningsesService, IMapper mapper)
        {
            _moviesService = moviesService;
            _screeningsService = screeningsesService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var movies = _mapper.Map<List<MovieViewModel>>(await _moviesService.GetLatestMoviesAsync());
            return View(movies);
        }

        public async Task<IActionResult> Details(int movieId)
        {
            var movie = await _moviesService.GetByIdAsync(movieId);
            var movieViewModel = _mapper.Map<MovieViewModel>(movie);

            // writing variable type out explicitly, because slight refactoring is required upon introducing pagination
            IReadOnlyCollection<Screening> screenings = (await _screeningsService.GetAllAsync(movieId, from: DateTime.Today)).Items;
            var screeningViewModels = _mapper.Map<List<ScreeningViewModel>>(screenings);

            return View(new MovieDetailViewModel()
            {
                Movie = movieViewModel,
                Screenings = screeningViewModels
            });
        }
    }
}
