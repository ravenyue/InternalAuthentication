using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace InternalAuthenticationSample.Controllers
{
    [Authorize]
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("Claims")]
        public IActionResult Claims()
        {
            var claims = HttpContext.User.Claims.Select(c => new { c.Type, c.Value });

            return Ok(claims);
        }

    }
}
