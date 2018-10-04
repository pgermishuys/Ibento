using System;
using System.Threading.Tasks;
using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Bus
{
    internal interface IMessageHandler
    {
        string HandlerName { get; }
        Task<bool> TryHandle(Message message);
        bool IsSame<T>(object handler);
    }

    internal class MessageHandler<T> : IMessageHandler where T : Message
    {
        public string HandlerName { get; private set; }

        private readonly IHandle<T> _handler;

        public MessageHandler(IHandle<T> handler, string handlerName)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            _handler = handler;
            HandlerName = handlerName ?? string.Empty;
        }

        public async Task<bool> TryHandle(Message message)
        {
            var msg = message as T;
            if (msg != null)
            {
                await _handler.Handle(msg);
                return true;
            }
            return false;
        }

        public bool IsSame<T2>(object handler)
        {
            return ReferenceEquals(_handler, handler) && typeof(T) == typeof(T2);
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(HandlerName) ? _handler.ToString() : HandlerName;
        }
    }
}