using Microsoft.AspNetCore.Mvc;

namespace BirthdayzBot.Controllers
{
    public class BirthdayzBotController : Controller
    {
        public IActionResult Updates()
        {
            return Content("Hi");
        }
    }
}
