using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace WebSite.Controllers
{
    public class DemoController : Controller
    {
        [HttpGet("demo1")]
        public ActionResult Demo1()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SwitchTheme()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            else
            {
                var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "demo_user") }, "demo");
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }

            return RedirectToAction("Demo1");
        }
    }
}
