using ELTE.Cinema.Shared.SignalR.Models;

namespace ELTE.Cinema.SignalR.Services;

public interface IScreeningsNotificationService
{
    Task AddToGroupAsync(string connectionId, string groupName);
    Task RemoveFromGroupAsync(string connectionId, string groupName);
    Task NotifySeatsStatusChangedAsync(int screeningId, List<SeatNotificationDto> seatNotificationDtos);
    Task NotifySeatStatusChangedAsync(int screeningId, SeatNotificationDto seatNotificationDto, string? senderConnectionId = null);
    Task NotifyOnClientConnectionAsync(string connectionId, int screeningId, bool isAdmin);
    Task NotifyOnClientDisconnectionAsync(string connectionId, int? screeningId = null);
}