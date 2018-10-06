using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ibento.DevelopmentHost.Bus;
using Ibento.DevelopmentHost.Messaging;
using Microsoft.AspNetCore.Http;

namespace Ibento.DevelopmentHost.Services
{
    public class HttpRequestManager : 
        IHandle<IncomingHttpRequestMessage>,
        IHandle<OutgoingHttpRequestMessage>
    {
        private readonly IDictionary<Guid, HttpContext> _messages;
        private readonly IPublisher _publisher;
        public HttpRequestManager(IPublisher publisher)
        {
            _publisher = publisher;
            _messages = new Dictionary<Guid, HttpContext>();
        }
        public async Task Handle(IncomingHttpRequestMessage message)
        {
            var messageId = Guid.NewGuid();
            _messages.Add(messageId, message.Entity);
            await _publisher.Publish(new AuthenticatedRequestMessage(messageId, message.Entity));
        }

        public async Task Handle(OutgoingHttpRequestMessage message)
        {
            var context = _messages[message.MessageId];
            context.Response.StatusCode = 200;
            var bytes = Encoding.UTF8.GetBytes("Success");
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            await context.Response.Body.FlushAsync();
        }
    }
}
