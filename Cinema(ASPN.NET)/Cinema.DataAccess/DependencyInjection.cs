using ELTE.Cinema.DataAccess.Config;
using ELTE.Cinema.DataAccess.Models;
using ELTE.Cinema.DataAccess.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ELTE.Cinema.DataAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration config)
    {
        // Config
        services.Configure<ReservationSettings>(config.GetSection("ReservationSettings"));
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        
        // Database
        var connectionString = config.GetConnectionString("DefaultConnection");
        services.AddDbContext<CinemaDbContext>(options => options
            .UseSqlServer(connectionString)
            .UseLazyLoadingProxies()
        );
        services.AddScoped<DbResetter>();
        
        //Identity
        services.AddIdentity<User, UserRole>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<CinemaDbContext>()
            .AddDefaultTokenProviders();

        // Services
        services.AddScoped<IMoviesService, MoviesService>();
        // services.AddScoped<IMoviesService, MoviesSqlService>();
        services.AddScoped<IRoomsService, RoomsService>();
        services.AddScoped<IScreeningsService, ScreeningsService>();
        services.AddScoped<IReservationsService, ReservationsService>();
        services.AddScoped<IUsersService, UsersService>();

        // Add email sending service
        services.AddSingleton<IEmailsService, SmtpEmailsService>();

        return services;
    }
}