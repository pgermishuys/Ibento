using System.Threading.Tasks;
using Ibento.DevelopmentHost.Bus;
using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Services
{
    public class IncomingHttpRequestAuthenticationManager : IHandle<IncomingHttpRequestMessage>
    {
        private readonly IPublisher _publisher;
        public IncomingHttpRequestAuthenticationManager(IPublisher publisher)
        {
            _publisher = publisher;
        }
        public async Task Handle(IncomingHttpRequestMessage message)
        {
            // if all checks out, then publish the next message
            await _publisher.Publish(new AuthenticatedRequestMessage(message.Entity));
        }
    }
}
