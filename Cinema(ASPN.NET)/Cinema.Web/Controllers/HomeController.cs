using System.Diagnostics;
using AutoMapper;
using ELTE.Cinema.DataAccess.Services;
using ELTE.Cinema.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace ELTE.Cinema.Web.Controllers;

public class HomeController : Controller
{
    private readonly IMoviesService _moviesService;
    private readonly IScreeningsService _screeningsService;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public HomeController(IMoviesService moviesService, IScreeningsService screeningsService, IMapper mapper, IConfiguration configuration)
    {
        _moviesService = moviesService;
        _screeningsService = screeningsService;
        _mapper = mapper;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {   
        var lastestMovies = await _moviesService.GetLatestMoviesAsync(int.Parse(_configuration["NewMovieCount"]!));
        var lastestMoviesViewModels = _mapper.Map<List<MovieViewModel>>(lastestMovies);

        var screenings = await _screeningsService.GetForDateAsync(DateTime.Now);
        var screeningViewModels = _mapper.Map<List<ScreeningViewModel>>(screenings);

        var homePageViewModel = new HomePageViewModel()
        {
            LatestMovies = lastestMoviesViewModels,
            TodayScreenings = screeningViewModels
        };

        return View(homePageViewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}