using System.Collections.Generic;
using System.Threading.Tasks;
using ELTE.TravelAgency.DataAccess.Models;

namespace ELTE.TravelAgency.DataAccess.Services;

public interface ICityService
{
    Task<IReadOnlyCollection<City>> GetAllAsync();
}