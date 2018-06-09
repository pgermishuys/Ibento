using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Bus
{
    public interface IQueuedHandler: IHandle<Message>, IPublisher
    {
        string Name { get; }
        void Start();
        void Stop();
        void RequestStop();
        QueueStats GetStatistics();
    }
}