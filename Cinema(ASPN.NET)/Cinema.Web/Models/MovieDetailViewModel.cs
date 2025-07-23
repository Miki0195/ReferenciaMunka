namespace ELTE.Cinema.Web.Models
{
    public class MovieDetailViewModel
    {
        public required MovieViewModel Movie { get; init; }
        public required List<ScreeningViewModel> Screenings { get; init; }
    }
}
