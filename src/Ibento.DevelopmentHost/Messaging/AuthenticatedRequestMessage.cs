using System.Threading;
using Microsoft.AspNetCore.Http;

namespace Ibento.DevelopmentHost.Messaging
{
    public class AuthenticatedRequestMessage : Message
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMsgId);
        public override int MsgTypeId => TypeId;
        public HttpContext Entity { get; }

        public AuthenticatedRequestMessage(HttpContext entity)
        {
            Entity = entity;
        }
    }
}
