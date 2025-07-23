using System.Threading.Tasks;
using ELTE.TravelAgency.DataAccess.Models;

namespace ELTE.TravelAgency.DataAccess.Services;

public interface IUserService
{
    Task RegistrateAsync(User user, string password);
    Task<User> LoginAsync(string email, string password);
    Task<User?> GetCurrentUserAsync();
    Task LogoutAsync();
}