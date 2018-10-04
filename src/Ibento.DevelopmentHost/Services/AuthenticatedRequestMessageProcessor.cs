using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ibento.DevelopmentHost.Bus;
using Ibento.DevelopmentHost.Framework;
using Ibento.DevelopmentHost.Messaging;
using Newtonsoft.Json;

namespace Ibento.DevelopmentHost.Services
{
    public class AuthenticatedRequestMessageProcessor : IHandle<AuthenticatedRequestMessage>
    {
        private readonly IPublisher _mainBus;
        private readonly MessageResolver _messageResolver;

        public AuthenticatedRequestMessageProcessor(IPublisher mainBus, MessageResolver messageResolver)
        {
            _mainBus = mainBus;
            _messageResolver = messageResolver;
        }

        public async Task Handle(AuthenticatedRequestMessage message)
        {
            if (message.Entity.Request.Headers.TryGetValue("mediaType", out var mediaType))
            {
                var messageType = _messageResolver.Resolve(mediaType);
                
                using (StreamReader reader = new StreamReader(message.Entity.Request.Body, Encoding.UTF8))
                {
                    var body = await reader.ReadToEndAsync();
                    var msg = (Message)JsonConvert.DeserializeObject(body, messageType);
                    await _mainBus.Publish(msg);
                    
                    message.Entity.Response.StatusCode = 200;
                    var bytes = Encoding.UTF8.GetBytes("Success");
                    await message.Entity.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    await message.Entity.Response.Body.FlushAsync();
                }
            }
            else
            {
                throw new Exception("Messages are required to be accompanied with a 'mediaType' header that describes the message type.");
            }
        }
    }
}
