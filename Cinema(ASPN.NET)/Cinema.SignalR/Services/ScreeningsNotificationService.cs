using ELTE.Cinema.Cache;
using ELTE.Cinema.Shared.SignalR.HubInterfaces;
using ELTE.Cinema.Shared.SignalR.Models;
using ELTE.Cinema.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ELTE.Cinema.SignalR.Services;

internal class ScreeningsNotificationService: IScreeningsNotificationService
{
    private readonly IHubContext<ScreeningsHub, IScreeningsHub> _hubContext;
    private readonly ICacheManager _cacheManager;

    public ScreeningsNotificationService(IHubContext<ScreeningsHub, IScreeningsHub> hubContext, ICacheManager cacheManager)
    {
        _hubContext = hubContext;
        _cacheManager = cacheManager;
    }

    
    public async Task AddToGroupAsync(string connectionId, string groupName)
    {
        await _hubContext.Groups.AddToGroupAsync(connectionId, groupName);
    }

    public async Task RemoveFromGroupAsync(string connectionId, string groupName)
    {
        await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName);

    }
    
    public async Task NotifySeatsStatusChangedAsync(int screeningId, List<SeatNotificationDto> seatNotificationDtos)
    {
        foreach (var seatSignalRDto in seatNotificationDtos)
        {
            await NotifySeatStatusChangedAsync(screeningId, seatSignalRDto);
        }
    }
    
    public async Task NotifySeatStatusChangedAsync(int screeningId, SeatNotificationDto seatNotificationDto, string? senderConnectionId = null)
    {
        await _hubContext.Clients.GroupExcept(ScreeningsHub.GetAdminGroupName(screeningId), senderConnectionId).SeatStatusChanged(screeningId, seatNotificationDto);

        seatNotificationDto.Reservation = null;
        await _hubContext.Clients.GroupExcept(screeningId.ToString(), senderConnectionId).SeatStatusChanged(screeningId, seatNotificationDto);
        
        await ManageCacheAsync(screeningId, seatNotificationDto, senderConnectionId);
    }
    
    public async Task NotifyOnClientConnectionAsync(string connectionId, int screeningId, bool isAdmin)
    {
        var selectedSeats = (await _cacheManager.GetAllByPatternAsync<List<SeatNotificationDto>>($"_{screeningId}"))
            .SelectMany(s => s.Value).ToList();
        foreach (var seat in selectedSeats)
        {
            if(!isAdmin)
                seat.Reservation = null;
            
            await _hubContext.Clients.Client(connectionId).SeatStatusChanged(screeningId, seat);
        }
    }

    public async Task NotifyOnClientDisconnectionAsync(string connectionId, int? screeningId = null)
    {
        var keyPattern = $"{connectionId}_{screeningId?.ToString() ?? ""}";
        var selectedSeatsOfClient = await _cacheManager.GetAllByPatternAsync<List<SeatNotificationDto>>(keyPattern);
        
        foreach (var seatDict in selectedSeatsOfClient)
        {
            seatDict.Value.ForEach(s => s.Status = SeatClientStatusDto.None);
            
            screeningId ??= int.Parse(seatDict.Key.Split('_')[1]);
            await NotifySeatsStatusChangedAsync(screeningId.Value, seatDict.Value);
        }
        
        await _cacheManager.RemoveAllByPatternAsync(keyPattern);
    }
    
    private async Task ManageCacheAsync(int screeningId, SeatNotificationDto seatNotificationDto, string? senderConnectionId)
    {
        if (senderConnectionId == null)
            return;

        switch (seatNotificationDto.Status)
        {
            case SeatClientStatusDto.Selected:
            {
                var key = GetSeatCacheKey(senderConnectionId, screeningId);
                var selectedSeats = await _cacheManager.GetAsync<List<SeatNotificationDto>>(key) ?? new List<SeatNotificationDto>();
                selectedSeats.Add(seatNotificationDto);
                await _cacheManager.SetAsync(key, selectedSeats);
                break;
            }
            case SeatClientStatusDto.None:
            {
                var key = GetSeatCacheKey(senderConnectionId, screeningId);
            
                var selectedSeats = await _cacheManager.GetAsync<List<SeatNotificationDto>>(key);
                var seat = selectedSeats?
                    .FirstOrDefault(s => s.Row == seatNotificationDto.Row && s.Column == seatNotificationDto.Column);

                if (seat != null)
                {
                    selectedSeats!.Remove(seat);
                }

                await _cacheManager.SetAsync(key, selectedSeats);
                break;
            }
        }
    }
    
    private static string GetSeatCacheKey(string connectionId, int screeningId)
        => $"{connectionId}_{screeningId}";
}