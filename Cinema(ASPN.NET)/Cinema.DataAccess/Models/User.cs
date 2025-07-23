using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ELTE.Cinema.DataAccess.Models;

public class User: IdentityUser
{
    [MaxLength(255)]
    public string Name { get; set; } = null!;
    public Guid? RefreshToken { get; set; }
}