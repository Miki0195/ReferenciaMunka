using Bomberman.Models.Database;
using Bomberman.Models.Profile;
using Bomberman.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bomberman.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IUserService _userService;
        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }
        public IActionResult Login(string? ReturnUrl)
        {
            return View(new Login { ReturnUrl = ReturnUrl });
        }
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(Login model)
        {
            if (!ModelState.IsValid)
                return View(model);

            User? u = _userService.GetUserByUsername(model.Username);

            if (u == null || _userService.EncryptPassword(model.Password) != u.Password)
            {
                HttpContext.Session.Remove("Username");
                model.ErrorMessage = "Incorrect Username or Password!";

                return View(model);
            }

            HttpContext.Session.SetString("Username", model.Username);

            var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, model.Username)
                };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            model.ErrorMessage = null!;

            if (!String.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Register(Register model)
        {
            //This should be a client side check
            if (model.Password != model.PasswordAgain)
            {
                HttpContext.Session.Remove("Username");
                //model.ErrorMessage = "The Passwords don't match!";
                return View(model);
            }

            string? message = _userService.TryRegister(new User
            {
                Username = model.Username,
                Password = model.Password,
                Email = model.Email
            });
            if (message == null)
            {
                HttpContext.Session.SetString("Username", model.Username);

                var claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, model.Username)
                };
                var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
                HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                //model.ErrorMessage = null!;
                return RedirectToAction("Index", "Home");
            }

            HttpContext.Session.Remove("Username");
            //model.ErrorMessage = message;
            return View(model);
        }
        [Authorize]
        public IActionResult Profile()
        {
            User currentUser = _userService.GetUserByUsername(HttpContext.User.Identity!.Name!)!;
            //Can implement multiple sorting modes if needed
            var scores = _userService.GetScoresForUser(currentUser)
                .OrderByDescending(x => x.Date)
                .ToList();

            return View(scores);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
