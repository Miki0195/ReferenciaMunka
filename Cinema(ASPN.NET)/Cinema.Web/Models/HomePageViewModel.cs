namespace ELTE.Cinema.Web.Models
{
    public class HomePageViewModel
    {
        public required List<MovieViewModel> LatestMovies { get; init; }
        public required List<ScreeningViewModel> TodayScreenings { get; init; }
    }
}
