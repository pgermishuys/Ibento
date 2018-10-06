using System;
using System.Threading;

namespace Ibento.DevelopmentHost.Messaging
{
    public class OutgoingHttpRequestMessage : Message
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMsgId);
        public override int MsgTypeId => TypeId;
        
        public OutgoingHttpRequestMessage(Guid messageId)
        {
            MessageId = messageId;
        }
    }
}
