using ELTE.Cinema.SignalR.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ELTE.Cinema.SignalR;

public static class DependencyInjection
{
    public static IServiceCollection AddSignalRServices(this IServiceCollection services)
    {
        services.AddSingleton<IMoviesNotificationService, MoviesNotificationService>();
        services.AddSingleton<IScreeningsNotificationService, ScreeningsNotificationService>();

        return services;
    }
}