namespace ELTE.Cinema.Shared.SignalR.Models;

public class SeatNotificationDto
{
    /// <summary>
    /// Row number of the seat
    /// </summary>
    public int Row { get; init; }

    /// <summary>
    /// Column number of the seat
    /// </summary>
    public int Column { get; init; }
    
    /// <summary>
    /// Status of the seat (None, Selected, Reserved, Sold)
    /// </summary>
    public SeatClientStatusDto Status { get; set; }
    
    /// <summary>
    /// Status of the seat (None, Selected, Reserved, Sold)
    /// </summary>
    public ReservationNotificationDto? Reservation { get; set; }
}