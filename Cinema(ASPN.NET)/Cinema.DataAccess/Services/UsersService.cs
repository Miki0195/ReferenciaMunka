using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ELTE.Cinema.DataAccess.Config;
using ELTE.Cinema.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ELTE.Cinema.DataAccess.Services;

internal class UsersService : IUsersService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public UsersService(
        IOptions<JwtSettings> jwtSettings, 
        IHttpContextAccessor httpContextAccessor, 
        UserManager<User> userManager, 
        SignInManager<User> signInManager)
    {
        _jwtSettings = jwtSettings.Value;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task AddUserAsync(User user, string password, Role? role = null)
    {
        user.RefreshToken = Guid.NewGuid();

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidDataException($"User creation failed: {result.Errors.First().Description}");

        if (role.HasValue)
        {
            result = await _userManager.AddToRoleAsync(user, role.Value.ToString());
            if (!result.Succeeded)
                throw new InvalidDataException("Adding to role failed");
        }
    }

    public async Task<(string authToken, string refreshToken, string userId)> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new AccessViolationException("Email or password is invalid");

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, false, true);
        if (result.IsLockedOut)
            throw new AccessViolationException("Too many failed attempt. User is locked out");
        if (!result.Succeeded)
            throw new AccessViolationException("Email or password is invalid");

        var accessToken = await GenerateJwtTokenAsync(user);
        
        return (accessToken, user.RefreshToken.ToString()!, user.Id);
    }
    
    public async Task<(string authToken, string refreshToken, string userId)> RedeemRefreshTokenAsync(string refreshToken)
    {
        if (!Guid.TryParse(refreshToken, out var parsedToken))
            throw new AccessViolationException("Invalid refresh token");

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == parsedToken);
        if(user == null)
            throw new AccessViolationException("Invalid refresh token");

        var accessToken = await GenerateJwtTokenAsync(user);

        return (accessToken, refreshToken, user.Id);
    }
    
    public async Task LogoutAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return;

        await _signInManager.SignOutAsync();
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return null;

        return await _userManager.FindByIdAsync(userId);
    }

    public string? GetCurrentUserId()
    {
        var id = _httpContextAccessor.HttpContext?.User.FindFirstValue("id");
        if (id == null)
            return null;

        return id;
    }
    
    public async Task<User> GetUserByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
            throw new AccessViolationException("User not accessible");

        if (!IsCurrentUserAdmin() && user.Id != GetCurrentUserId())
            throw new AccessViolationException("User not accessible");

        return user;
    }

    public List<Role> GetCurrentUserRoles()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null)
            return [];

        var roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        return roles.Select(Enum.Parse<Role>).ToList();
    }

    public bool IsCurrentUserAdmin()
    {
        var roles = GetCurrentUserRoles();

        return roles.Contains(Role.Admin);
    }

    private async Task<string> GenerateJwtTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("id", user.Id),
            new("username", user.UserName!),
        };

        var userRoles = await _userManager.GetRolesAsync(user);
        foreach (var userRole in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}