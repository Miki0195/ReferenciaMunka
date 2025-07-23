using ELTE.Cinema.Shared.SignalR.Models;

namespace ELTE.Cinema.SignalR.Services;

public interface IMoviesNotificationService
{
    Task NotifyNewMovieAddedAsync(MovieNotificationDto movieNotificationDto);
}