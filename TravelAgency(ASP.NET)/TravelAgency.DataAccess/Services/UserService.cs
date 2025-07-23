using System;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ELTE.TravelAgency.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace ELTE.TravelAgency.DataAccess.Services;

public class UserService: IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public UserService(UserManager<User> userManager, SignInManager<User> signInManager, IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task RegistrateAsync(User user, string password)
    {
        var existingUser = await _userManager.FindByEmailAsync(user.Email!);
        if (existingUser != null)
            throw new InvalidDataException("Az email cím már foglalt");

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidDataException($"A felhasználó létrehozása sikertelen.");
    }

    public async Task<User> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new AccessViolationException("A felhasználónév vagy jelszó hibás");

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, false, true);
        if (result.IsLockedOut)
            throw new AccessViolationException("Túl sok sikertelen kísérlet");
        if (!result.Succeeded)
            throw new AccessViolationException("A felhasználónév vagy jelszó hibás");

        await _signInManager.SignInAsync(user, isPersistent: false);

        // access token beállítása
        user.ResetAccessToken();
        await _userManager.UpdateAsync(user);

        return user;
    }

    public async Task<User?> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return null;

        return await _userManager.FindByIdAsync(userId);
    }

    private string? GetCurrentUserId()
    {
        var id = _httpContextAccessor.HttpContext?.User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (id == null)
            return null;

        return id;
    }

    public async Task LogoutAsync()
    {
        var user = await GetCurrentUserAsync();
        if (user != null)
        {
            // korábbi access token érvénytelenítése
            user.ResetAccessToken();
            await _userManager.UpdateAsync(user);
        }
        await _signInManager.SignOutAsync();
    }
}