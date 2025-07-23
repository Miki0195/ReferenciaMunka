using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ELTE.TravelAgency.DataAccess;
using ELTE.TravelAgency.DataAccess.Services;
using ELTE.TravelAgency.Web;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TravelAgencyContext>(options => // Dependency injection beállítása az adatbázis kontextushoz
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("TravelAgency.DataAccess")));

// Dependency injection beállítása a Google konfiguráció kollekcióhoz
builder.Services.Configure<GoogleConfig>(builder.Configuration.GetSection("Google"));

// Dependency injection beállítása az utazással kapcsolatos szolgáltatásokhoz
builder.Services.AddTransient<ICityService, CityService>();
builder.Services.AddTransient<IBuildingService, BuildingService>();
builder.Services.AddTransient<IApartmentService, ApartmentService>();
builder.Services.AddTransient<IRentService, RentService>();

// Add AutoMapper to container. Pass the assembly of the profiles.
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
// Alternative: it will automatically get the assembly of the given type.
//builder.Services.AddAutoMapper(typeof(Program));


builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var serviceScope = app.Services.CreateScope())
{
    // Adatbázis inicializálása
    DbInitializer.Initialize(
        serviceScope.ServiceProvider,
        builder.Configuration.GetValue<string>("ImageStore"));
}

app.Run();