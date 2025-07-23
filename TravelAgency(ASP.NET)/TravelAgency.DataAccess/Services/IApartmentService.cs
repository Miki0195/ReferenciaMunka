using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ELTE.TravelAgency.DataAccess.Models;

namespace ELTE.TravelAgency.DataAccess.Services;

public interface IApartmentService
{
    Task<IReadOnlyCollection<Apartment>> GetAllAsync();
    Task<Apartment> GetAsync(int id);
    Task<int> GetPrice(DateTime start, DateTime end, int id);
    Task<RentDateError> IsAvailable(DateTime start, DateTime end, int id);
    Task AddAsync(Apartment apartment);
    Task UpdateAsync(Apartment apartment);
    Task DeleteAsync(int id);
}