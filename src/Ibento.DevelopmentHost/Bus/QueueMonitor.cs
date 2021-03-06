﻿using System.Collections.Concurrent;
using System.Linq;

namespace Ibento.DevelopmentHost.Bus
{
    public class QueueMonitor
    {
        public static readonly QueueMonitor Default = new QueueMonitor();

        private readonly ConcurrentDictionary<IMonitoredQueue, IMonitoredQueue> _queues = new ConcurrentDictionary<IMonitoredQueue, IMonitoredQueue>();

        private QueueMonitor()
        {
        }

        public void Register(IMonitoredQueue monitoredQueue)
        {
            _queues[monitoredQueue] = monitoredQueue;
        }

        public void Unregister(IMonitoredQueue monitoredQueue)
        {
            IMonitoredQueue v;
            _queues.TryRemove(monitoredQueue, out v);
        }

        public QueueStats[] GetStats()
        {
            var stats = _queues.Keys.OrderBy(x => x.Name).Select(queue => queue.GetStatistics()).ToArray();
            return stats;
        }
    }
}