using ELTE.Cinema.Shared.SignalR.Models;
using ELTE.Cinema.SignalR.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ELTE.Cinema.SignalR.Services;

internal class MoviesNotificationService: IMoviesNotificationService
{
    private readonly IHubContext<MoviesHub> _hubContext;

    public MoviesNotificationService(IHubContext<MoviesHub> hubContext)
    {
        _hubContext = hubContext;
    }
    public async Task NotifyNewMovieAddedAsync(MovieNotificationDto movieNotificationDto)
    {
        await _hubContext.Clients.All.SendAsync("NewMovieAdded", movieNotificationDto);
    }
}