using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebSite.Services;

namespace WebSite.Controllers
{
    public class DemoController : Controller
    {
        private readonly DemoService _service;

        public DemoController(DemoService service)
        {
            _service = service;
        }

        [HttpGet("demo")]
        public ActionResult Demo()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SwitchTheme()
        {
            await _service.SwitchThemeAsync(HttpContext);
            return RedirectToAction("Demo");
        }

    }
}
