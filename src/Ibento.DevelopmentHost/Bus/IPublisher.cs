using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Bus
{
    public interface IPublisher
    {
        void Publish(Message message);
    }

    public interface IThreadSafePublisher 
    {
    }
}