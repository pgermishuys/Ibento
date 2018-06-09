namespace Ibento.DevelopmentHost.Bus
{
    public interface IBus: IPublisher, ISubscriber
    {
        string Name { get; }
    }
}