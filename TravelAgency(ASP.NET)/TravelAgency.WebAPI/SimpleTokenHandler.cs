using System.Data;
using System.Security.Claims;
using System.Text.Encodings.Web;
using ELTE.TravelAgency.DataAccess.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ELTE.TravelAgency.WebAPI
{
    public class SimpleTokenHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly UserManager<User> _userManager;

        public SimpleTokenHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            UserManager<User> userManager)
            : base(options, logger, encoder)
        {
            _userManager = userManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return AuthenticateResult.NoResult();

            var token = authHeader.Substring("Bearer ".Length).Trim();

            var parts = token.Split('|');
            if (parts.Length != 2)
                return AuthenticateResult.Fail("Invalid Token Format");

            var user =  await _userManager.FindByEmailAsync(parts[0]);
            if (user == null || user.AccessToken != parts[1])
                return AuthenticateResult.Fail("Invalid Token");

            var claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
