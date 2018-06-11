using System.Threading;
using Ibento.DevelopmentHost.Bus;
using Microsoft.AspNetCore.Http;

namespace Ibento.DevelopmentHost.Messaging
{
    public class IncomingHttpRequestMessage : Message
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMsgId);
        public override int MsgTypeId => TypeId;

        public readonly IPublisher NextStagePublisher;
        public readonly HttpContext Entity;

        public IncomingHttpRequestMessage(HttpContext entity, IPublisher nextStagePublisher)
        {
            Entity = entity;
            NextStagePublisher = nextStagePublisher;
        }
    }
}
