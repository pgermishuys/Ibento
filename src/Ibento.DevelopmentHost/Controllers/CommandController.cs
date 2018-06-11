using System.Threading.Tasks;
using Ibento.DevelopmentHost.Bus;
using Ibento.DevelopmentHost.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace Ibento.DevelopmentHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        private readonly IPublisher _mainBus;
        public CommandController(IPublisher mainBus)
        {
            _mainBus = mainBus;
        }
        [HttpPost]
        public void ProcessRequest()
        {
            _mainBus.Publish(new IncomingHttpRequestMessage(HttpContext, _mainBus));
        }
    }
}
