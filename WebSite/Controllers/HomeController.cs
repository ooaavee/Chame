using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebSite.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            bool isAuthenticated = HttpContext.User.Identity.IsAuthenticated;

            if (!isAuthenticated)
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity("ChameDemo")));
            }
            else
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
 

            //context.Authenticate | Challenge | SignInAsync("scheme"); // Calls 2.0 auth stack




            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
