using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Bus
{
    public interface IHandle<T> where T: Message
    {
        void Handle(T message);
    }
}