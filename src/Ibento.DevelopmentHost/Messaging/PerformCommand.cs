using System.Threading;
using Microsoft.AspNetCore.Http;

namespace Ibento.DevelopmentHost.Messaging
{
    public class PerformCommand : Message
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMsgId);
        public override int MsgTypeId => TypeId;
        public HttpContext Entity { get; }

        public PerformCommand(HttpContext entity)
        {
            Entity = entity;
        }
    }
}
