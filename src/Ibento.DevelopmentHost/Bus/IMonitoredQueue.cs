namespace Ibento.DevelopmentHost.Bus
{
    public interface IMonitoredQueue
    {
        string Name { get; }
        QueueStats GetStatistics();
    }
}