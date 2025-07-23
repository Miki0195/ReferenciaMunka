using ELTE.Cinema.DataAccess.Models;

namespace ELTE.Cinema.DataAccess.Services;

public interface IUsersService
{
    Task AddUserAsync(User user, string password, Role? role = null);
    Task<(string authToken, string refreshToken, string userId)> LoginAsync(string email, string password);
    Task<(string authToken, string refreshToken, string userId)> RedeemRefreshTokenAsync(string refreshToken);
    Task LogoutAsync();
    Task<User?> GetCurrentUserAsync();
    string? GetCurrentUserId();
    Task<User> GetUserByIdAsync(string id);
    List<Role> GetCurrentUserRoles();
    bool IsCurrentUserAdmin();
}