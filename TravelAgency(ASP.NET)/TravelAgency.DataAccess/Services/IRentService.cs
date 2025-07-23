using System.Collections.Generic;
using System.Threading.Tasks;
using ELTE.TravelAgency.DataAccess.Models;

namespace ELTE.TravelAgency.DataAccess.Services;

public interface IRentService
{
    Task<IReadOnlyCollection<Rent>> GetAllAsync(int? apartmentId = null);
    Task<Rent> GetAsync(int id);
    Task AddAsync(Rent rent);
    Task UpdateAsync(Rent rent);
    Task DeleteAsync(int id);
}