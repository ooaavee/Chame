using System.Runtime.InteropServices.ComTypes;
using System.Security.Claims;
using System.Threading.Tasks;
using Chame;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebSite.Controllers
{
    public class DemoController : Controller
    {
        private readonly IChameService _chameService;

        public DemoController(IChameService chameService)
        {
            _chameService = chameService;
        }

        [HttpGet("demo1")]
        public async Task<ActionResult> Demo1()
        {

            var data = await _chameService.GetContentAsync("indeax.html");


            return View("Demo1/Demo1");
        }

        [HttpPost]
        public async Task<ActionResult> SetThemeA()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Demo1");
        }

        [HttpPost]
        public async Task<ActionResult> SetThemeB()
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "themeB") }, "demo");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToAction("Demo1");
        }

        [HttpPost]
        public async Task<ActionResult> SetThemeC()
        {
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "themeC") }, "demo");
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            return RedirectToAction("Demo1");
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
