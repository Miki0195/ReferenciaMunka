using Microsoft.AspNetCore.Identity;

namespace ELTE.Cinema.DataAccess.Models;

public class UserRole: IdentityRole
{
    public UserRole() { }
    public UserRole(string role): base(role) { }
}