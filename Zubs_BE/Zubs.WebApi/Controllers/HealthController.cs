using Microsoft.AspNetCore.Mvc;

namespace Zubs.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet("health")]
        [HttpHead("health")]
        public ActionResult Health()
        {
            return Ok("Healthy");
        }

    }

}
