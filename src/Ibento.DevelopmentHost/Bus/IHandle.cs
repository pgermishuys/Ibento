using System.Threading.Tasks;
using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Bus
{
    public interface IHandle<T> where T: Message
    {
        Task Handle(T message);
    }
}