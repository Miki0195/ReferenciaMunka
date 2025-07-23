using System.Collections.Generic;
using System.Threading.Tasks;
using ELTE.TravelAgency.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ELTE.TravelAgency.DataAccess.Services;

public class CityService : ICityService
{
    private readonly TravelAgencyContext _context;

    public CityService(TravelAgencyContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<City>> GetAllAsync()
    {
        return await _context.Cities.ToListAsync();
    }
}