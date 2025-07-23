namespace ELTE.Cinema.Web.Models
{
    public class ReservationPageViewModel
    {
        public required ScreeningViewModel ScreeningViewModel { get; init; }
        public required List<SeatViewModel> SeatViewModels { get; init; }
        public ReservationViewModel? ReservationViewModel { get; init; }
        public required int MaximumNumberOfSeats { get; init; }
    }
}
