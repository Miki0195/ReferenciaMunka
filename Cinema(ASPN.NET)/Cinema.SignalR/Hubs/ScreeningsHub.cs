using System.Security.Claims;
using ELTE.Cinema.Shared.SignalR.HubInterfaces;
using ELTE.Cinema.Shared.SignalR.Models;
using ELTE.Cinema.SignalR.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ELTE.Cinema.SignalR.Hubs;

[Authorize]
public class ScreeningsHub : Hub<IScreeningsHub>
{
    private readonly IScreeningsNotificationService _notificationService;
    public ScreeningsHub(IScreeningsNotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    
    public override Task OnDisconnectedAsync(Exception exception)
    {
        _notificationService.NotifyOnClientDisconnectionAsync(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
    
    public async Task JoinScreeningGroup(int screeningId)
    {
        var isAdmin = IsAdmin();
        var groupName = isAdmin ? GetAdminGroupName(screeningId): screeningId.ToString();
        await _notificationService.AddToGroupAsync(Context.ConnectionId, groupName);
        
        await _notificationService.NotifyOnClientConnectionAsync(Context.ConnectionId, screeningId, isAdmin);
    }

    public async Task LeaveScreeningGroup(int screeningId)
    {
        var groupName = IsAdmin() ? GetAdminGroupName(screeningId): screeningId.ToString();
        await _notificationService.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        await _notificationService.NotifyOnClientDisconnectionAsync(Context.ConnectionId, screeningId);
    }
    
    public async Task SeatStatusChanged(int screeningId, SeatNotificationDto seatNotificationDto)
    {
        await _notificationService.NotifySeatStatusChangedAsync(screeningId, seatNotificationDto, Context.ConnectionId);
    }
    
    private bool IsAdmin()
    {
        var roles = Context.User?.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        var isAdmin = roles != null && roles.Contains("Admin");
        
        return isAdmin;
    }
    
    public static string GetAdminGroupName(int screeningId)
        => $"{screeningId}_admin";
}