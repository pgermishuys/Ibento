using System.Threading.Tasks;
using Ibento.DevelopmentHost.Bus;
using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Services
{
    public class PerformCommandService : IHandle<PerformCommand>
    {
        private IPublisher _publisher;

        public PerformCommandService(IPublisher publisher)
        {
            _publisher = publisher;
        }
        public async Task Handle(PerformCommand message)
        {
            await _publisher.Publish(new OutgoingHttpRequestMessage(message.MessageId));
        }
    }
}
