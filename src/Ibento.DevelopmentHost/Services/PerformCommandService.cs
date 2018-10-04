using System.Threading.Tasks;
using Ibento.DevelopmentHost.Bus;
using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Services
{
    public class PerformCommandService : IHandle<PerformCommand>
    {
        private IPublisher _mainBus;

        public PerformCommandService(IPublisher mainBus)
        {
            _mainBus = mainBus;
        }
        public Task Handle(PerformCommand message)
        {
            return Task.FromResult(0);
        }
    }
}
