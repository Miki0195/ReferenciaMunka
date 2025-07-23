using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ELTE.TravelAgency.DataAccess.Models;

[Index(nameof(AccessToken), IsUnique = true)]
public class User: IdentityUser
{
    [MaxLength(255)]
    public required string Name { get; set; }

    [MaxLength(255)]
    public required string Address { get; set; }

    [MaxLength(100)]
    public string AccessToken { get; set; } = GenerateOpaqueToken();

    public void ResetAccessToken()
    {
        AccessToken = GenerateOpaqueToken();
    }

    private static string GenerateOpaqueToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}