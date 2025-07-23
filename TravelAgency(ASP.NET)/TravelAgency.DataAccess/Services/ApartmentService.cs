using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ELTE.TravelAgency.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ELTE.TravelAgency.DataAccess.Services;

public class ApartmentService : IApartmentService
{
    private readonly TravelAgencyContext _context;

    public ApartmentService(TravelAgencyContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Apartment>> GetAllAsync()
    {
        return await _context.Apartments.ToListAsync();
    }

    public async Task<Apartment> GetAsync(int id)
    {
        var apartment = await _context.Apartments
            .Include(a => a.Building) // betöltjük az apartmanhoz az épületeket
            .ThenInclude(b => b.City) // az épülethez pedig a várost
            .FirstAsync(apartment => apartment.Id == id);

        return apartment;
    }

    public async Task<int> GetPrice(DateTime start, DateTime end, int id)
    {
        Apartment apartment = await GetAsync(id);
        return apartment.Price * Convert.ToInt32((end - start).TotalDays);
    }

    public async Task<RentDateError> IsAvailable(DateTime start, DateTime end, int id)
    {
        Apartment? apartment = await _context.Apartments.FirstOrDefaultAsync(apartment => apartment.Id == id);
        if (apartment == null)
            return RentDateError.ApartmentNotExists; // nem létező apartman

        if (start < DateTime.Now + TimeSpan.FromDays(7)) // korai kezdés
            return RentDateError.StartInvalid;

        if (end < start)
            return RentDateError.EndInvalid;

        if (end == start) // üres foglalás 
            return RentDateError.LengthInvalid;

        if (Convert.ToInt32((end - start).TotalDays) % 7 != 0) // nem egész hetet foglalt
            return RentDateError.LengthInvalid;

        if (start.DayOfWeek != apartment.Turnday) // nem fordulónapos kezdés
            return RentDateError.StartInvalid;

        if (_context.Rents.Where(r => r.ApartmentId == apartment.Id && r.EndDate >= start)
            .ToList()
            .Any(r => r.IsConflicting(start, end))) // az időszakra már van foglalás
            return RentDateError.Conflicting;

        return RentDateError.None; // ha ideág eljutunk, nem találtunk hibát.
    }

    public async Task AddAsync(Apartment apartment)
    {
        await _context.Apartments.AddAsync(apartment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Apartment apartment)
    {
        if (!await _context.Apartments.AnyAsync(r => r.Id == apartment.Id))
            throw new ArgumentException("Apartment does not exist.", nameof(apartment));

        _context.Apartments.Update(apartment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var apartment = await GetAsync(id);
        _context.Apartments.Remove(apartment);
        await _context.SaveChangesAsync();
    }
}