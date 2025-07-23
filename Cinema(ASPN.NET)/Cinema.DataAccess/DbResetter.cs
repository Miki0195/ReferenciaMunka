using ELTE.Cinema.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Respawn;

namespace ELTE.Cinema.DataAccess;

public class DbResetter
{
    private readonly string _connectionString;
    private readonly string _imagePath;
    private readonly CinemaDbContext _context;
    private readonly RoleManager<UserRole> _roleManager;
    private readonly UserManager<User> _userManager;
    
    public DbResetter(IConfiguration config, CinemaDbContext context, RoleManager<UserRole> roleManager, UserManager<User> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _connectionString = config.GetConnectionString("DefaultConnection")!;
        _imagePath = config.GetSection("SeedSettings").GetValue<string>("ImageSource")!;
    }

    public async Task ResetAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            TablesToIgnore = [ "__EFMigrationsHistory" ],
        });
        await respawner.ResetAsync(_connectionString);
        
        DbInitializer.Initialize(_context, _imagePath, _roleManager, _userManager);
    }
}