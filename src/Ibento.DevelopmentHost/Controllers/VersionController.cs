using Microsoft.AspNetCore.Mvc;

namespace Ibento.DevelopmentHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public ActionResult<string> Get()
        {
            return Ok(VersionInfo.Version);
        }
    }
}
