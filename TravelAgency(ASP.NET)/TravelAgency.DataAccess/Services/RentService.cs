using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ELTE.TravelAgency.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace ELTE.TravelAgency.DataAccess.Services;

public class RentService : IRentService
{
    private readonly TravelAgencyContext _context;

    public RentService(TravelAgencyContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Rent>> GetAllAsync(int? apartmentId)
    {
        return await _context.Rents
            .Where(r => !apartmentId.HasValue || r.ApartmentId == apartmentId)
            .ToListAsync();
    }

    public async Task<Rent> GetAsync(int id)
    {
        var rent = await _context.Rents.FindAsync(id);
        if (rent is null)
            throw new ArgumentException("Rent does not exist.", nameof(id));

        return rent;
    }

    public async Task AddAsync(Rent rent)
    {
        _context.Rents.Attach(rent);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Rent rent)
    {
        if (!await _context.Rents.AnyAsync(r => r.Id == rent.Id))
            throw new ArgumentException("Rent does not exist.", nameof(rent));

        _context.Rents.Update(rent);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var rent = await GetAsync(id);
        _context.Rents.Remove(rent);
        await _context.SaveChangesAsync();
    }
}