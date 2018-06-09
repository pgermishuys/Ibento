using System;
using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Bus
{
    public class QueuedHandler : QueuedHandlerMRES, IQueuedHandler
    {
        public static IQueuedHandler CreateQueuedHandler(IHandle<Message> consumer, string name,
            bool watchSlowMsg = true,
            TimeSpan? slowMsgThreshold = null, TimeSpan? threadStopWaitTimeout = null, string groupName = null)
        {
            return new QueuedHandler(consumer, name, watchSlowMsg, slowMsgThreshold, threadStopWaitTimeout, groupName);
        }

        public static readonly TimeSpan DefaultStopWaitTimeout = TimeSpan.FromSeconds(10);
        public static readonly TimeSpan VerySlowMsgThreshold = TimeSpan.FromSeconds(7);

        QueuedHandler(IHandle<Message> consumer,
            string name,
            bool watchSlowMsg = true,
            TimeSpan? slowMsgThreshold = null,
            TimeSpan? threadStopWaitTimeout = null,
            string groupName = null)
            : base(
                consumer, name, watchSlowMsg, slowMsgThreshold, threadStopWaitTimeout ?? DefaultStopWaitTimeout,
                groupName)
        {
        }
    }
}