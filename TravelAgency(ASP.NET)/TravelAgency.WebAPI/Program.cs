using ELTE.TravelAgency.DataAccess;
using ELTE.TravelAgency.DataAccess.Models;
using ELTE.TravelAgency.DataAccess.Services;
using ELTE.TravelAgency.WebAPI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
    {
        options.Filters.Add(new ProducesAttribute("application/json"));

    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TravelAgency API",
        Version = "v1",
        Description = "ELTE TravelAgency API"
    });
    var xfile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xpath = Path.Combine(AppContext.BaseDirectory, xfile);
    c.IncludeXmlComments(xpath);
});

// Add services to the container.
builder.Services.AddDbContext<TravelAgencyContext>(options => // Dependency injection beállítása az adatbázis kontextushoz
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("TravelAgency.DataAccess")));

// Dependency injection beállítása az utazással kapcsolatos szolgáltatásokhoz
builder.Services.AddTransient<ICityService, CityService>();
builder.Services.AddTransient<IBuildingService, BuildingService>();
builder.Services.AddTransient<IApartmentService, ApartmentService>();
builder.Services.AddTransient<IRentService, RentService>();
builder.Services.AddTransient<IUserService, UserService>();


// Add AutoMapper to container. Pass the assembly of the profiles.
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
// Alternative: it will automatically get the assembly of the given type.
//builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddIdentity<User, IdentityRole>(options =>
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
    .AddEntityFrameworkStores<TravelAgencyContext>()
    .AddDefaultTokenProviders();

// Indentity süti beállítása
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.LoginPath = null;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

// Configure multi scheme authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "MultiScheme";
        options.DefaultChallengeScheme = "MultiScheme";
    })
    .AddScheme<AuthenticationSchemeOptions, SimpleTokenHandler>("Token", null)
    .AddPolicyScheme("MultiScheme", "Multi Auth", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            // Use token if there's an Authorization header, else use Cookies
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                return "Token";

            return "Identity.Application";
        };
    });

// Configure CORS policy for Blazor client
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy",
        policy =>
        {
            policy.WithOrigins(builder.Configuration["BlazorUrl"]
                               ?? throw new ArgumentNullException("BlazorUrl")) // Enable CORS for Blazor client
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("E2E"))
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Enable CORS for development
    app.UseCors("BlazorPolicy");
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// Az integrációs teszteknek saját seedjük van
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var serviceScope = app.Services.CreateScope())
    {
        // Adatbázis inicializálása
        DbInitializer.Initialize(
            serviceScope.ServiceProvider,
            builder.Configuration.GetValue<string>("ImageStore"));
    }
}

app.Run();