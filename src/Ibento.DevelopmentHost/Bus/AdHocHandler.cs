using System;
using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Bus
{
    public class AdHocHandler<T>: IHandle<T> where T: Message
    {
        private readonly Action<T> _handle;

        public AdHocHandler(Action<T> handle)
        {
            _handle = handle;
        }

        public void Handle(T message)
        {
            _handle(message);
        }
    }
}