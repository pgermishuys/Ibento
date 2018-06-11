using System;
using System.Text;
using Ibento.DevelopmentHost.Bus;
using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Services
{
    public class AuthenticatedRequestMessageProcessor : IHandle<AuthenticatedRequestMessage>
    {
        private readonly IPublisher _mainBus;

        public AuthenticatedRequestMessageProcessor(IPublisher mainBus)
        {
            _mainBus = mainBus;
        }

        public void Handle(AuthenticatedRequestMessage message)
        {
            _mainBus.Publish(new WriteLogbookEntryMessage(message.Entity));
        }
    }
}
