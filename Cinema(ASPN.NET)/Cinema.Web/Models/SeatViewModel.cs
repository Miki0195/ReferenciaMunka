namespace ELTE.Cinema.Web.Models
{
    public class SeatViewModel
    {
        public int Row { get; init; }
        public int Column { get; init; }
        public SeatViewModelStatus Status { get; set; }
    }
}
