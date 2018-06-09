using System.Threading;

namespace Ibento.DevelopmentHost.Messaging
{
    public static class SystemMessage
    {
        public class SystemInit : Message
        {
            private static readonly int TypeId = Interlocked.Increment(ref NextMsgId);

            public override int MsgTypeId
            {
                get { return TypeId; }
            }
        }
    }
}