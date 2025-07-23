using Bomberman.Models.Database;
using Microsoft.EntityFrameworkCore;
using Bomberman.Models.SignalR;
using Bomberman.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Bomberman
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();
            builder.Services.AddScoped<IUserService, UserService>();

			builder.Services.AddDbContext<BombermanDbContext>(options =>
            {
                //Found in appsettings.json
                switch (builder.Configuration.GetSection("DbMode").Value)
                {
                    case "SqlServer":
                        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
                        break;
                    case "MySql":
                        options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnection"), ServerVersion.Create(new Version(10, 11, 6), ServerType.MariaDb));
                        break;
                    case "MySqlMiki":
                        options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnectionMiki"), ServerVersion.Create(new Version(10, 11, 6), ServerType.MariaDb));
                        break;
                    case "Docker":
                        options.UseMySql(builder.Configuration.GetConnectionString("DockerMySqlConnection"), ServerVersion.Create(new Version(11, 4, 2), ServerType.MariaDb), options => options.EnableRetryOnFailure());
                        break;
                }
            });

            //HttpContext.Session and Cookies
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();
            builder.Services.AddAuthentication(options => options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.LoginPath = "/Profile/Login";
                    options.LogoutPath = "/Home/Index";
                    options.AccessDeniedPath = "/Profile/Login";
                });
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
			}
            else //Deployment settings
            {
				app.UseHttpsRedirection();
				app.UseHsts();
			}

			app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<GameHub>("/Play");

            using (var serviceScope = app.Services.CreateScope())
            using (var context = serviceScope.ServiceProvider.GetRequiredService<BombermanDbContext>())
            {
                var service = serviceScope.ServiceProvider.GetRequiredService<IUserService>();
                DbInitializer.Initialize(context, service);
            }

            app.Run();
        }
    }
}
