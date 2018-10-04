using System.Threading.Tasks;
using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Bus
{
    public interface IPublisher
    {
        Task Publish(Message message);
    }

    public interface IThreadSafePublisher 
    {
    }
}