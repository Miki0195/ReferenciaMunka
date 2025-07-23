using ELTE.Cinema.Shared.SignalR.Models;

namespace ELTE.Cinema.Shared.SignalR.HubInterfaces;

public interface IScreeningsHub
{
    Task JoinScreeningGroup(int screeningId);
    Task LeaveScreeningGroup(int screeningId);
    Task SeatStatusChanged(int screeningId, SeatNotificationDto  seatNotificationDto);
}