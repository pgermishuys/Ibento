using System;
using System.Collections.Concurrent;
using System.Threading;
using Ibento.DevelopmentHost.Messaging;
using Serilog;

namespace Ibento.DevelopmentHost.Bus
{
    public class QueuedHandlerMRES : IQueuedHandler, IMonitoredQueue, IThreadSafePublisher
    {
        public int MessageCount { get { return _queue.Count; } }
        public string Name { get { return _queueStats.Name; } }

        private readonly IHandle<Message> _consumer;

        private readonly bool _watchSlowMsg;
        private readonly TimeSpan _slowMsgThreshold;

        private readonly ConcurrentQueue<Message> _queue = new ConcurrentQueue<Message>();
        private readonly ManualResetEventSlim _msgAddEvent = new ManualResetEventSlim(false);

        private Thread _thread;
        private volatile bool _stop;
        private volatile bool _starving;
        private readonly ManualResetEventSlim _stopped = new ManualResetEventSlim(true);
        private readonly TimeSpan _threadStopWaitTimeout;

        private readonly QueueMonitor _queueMonitor;
        private readonly QueueStatsCollector _queueStats;
        
        public QueuedHandlerMRES(IHandle<Message> consumer,
                                 string name,
                                 bool watchSlowMsg = true,
                                 TimeSpan? slowMsgThreshold = null,
                                 TimeSpan? threadStopWaitTimeout = null,
                                 string groupName = null)
        {
            _consumer = consumer;

            _watchSlowMsg = watchSlowMsg;
            _slowMsgThreshold = slowMsgThreshold ?? InMemoryBus.DefaultSlowMessageThreshold;
            _threadStopWaitTimeout = threadStopWaitTimeout ?? QueuedHandler.DefaultStopWaitTimeout;

            _queueMonitor = QueueMonitor.Default;
            _queueStats = new QueueStatsCollector(name, groupName);
        }

        public void Start()
        {
            if (_thread != null)
                throw new InvalidOperationException("Already a thread running.");

            _queueMonitor.Register(this);

            _stopped.Reset();

            _thread = new Thread(ReadFromQueue) { IsBackground = true, Name = Name };
            _thread.Start();
        }

        public void Stop()
        {
            _stop = true;
            if (!_stopped.Wait(_threadStopWaitTimeout))
                throw new TimeoutException(string.Format("Unable to stop thread '{0}'.", Name));
        }

        public void RequestStop()
        {
            _stop = true;
        }

        private void ReadFromQueue(object o)
        {
            _queueStats.Start();
            Thread.BeginThreadAffinity(); // ensure we are not switching between OS threads. Required at least for v8.
            
            while (!_stop)
            {
                Message msg = null;
                try
                {
                    if (!_queue.TryDequeue(out msg))
                    {
                        _starving = true;

                        _queueStats.EnterIdle();
                        _msgAddEvent.Wait(100);
                        _msgAddEvent.Reset();

                        _starving = false;
                    }
                    else
                    {
                        _queueStats.EnterBusy();
#if DEBUG
                        _queueStats.Dequeued(msg);
#endif

                        var cnt = _queue.Count;
                        _queueStats.ProcessingStarted(msg.GetType(), cnt);

                        if (_watchSlowMsg)
                        {
                            var start = DateTime.UtcNow;

                            _consumer.Handle(msg);

                            var elapsed = DateTime.UtcNow - start;
                            if (elapsed > _slowMsgThreshold)
                            {
                                Log.Debug("SLOW QUEUE MSG [{0}]: {1} - {2}ms. Q: {3}/{4}.",
                                          Name, _queueStats.InProgressMessage.Name, (int)elapsed.TotalMilliseconds, cnt, _queue.Count);
                                if (elapsed > QueuedHandler.VerySlowMsgThreshold && !(msg is SystemMessage.SystemInit))
                                    Log.Error("---!!! VERY SLOW QUEUE MSG [{0}]: {1} - {2}ms. Q: {3}/{4}.",
                                              Name, _queueStats.InProgressMessage.Name, (int)elapsed.TotalMilliseconds, cnt, _queue.Count);
                            }
                        }
                        else
                        {
                            _consumer.Handle(msg);
                        }

                        _queueStats.ProcessingEnded(1);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while processing message {0} in queued handler '{1}'.", msg, Name);
                }
            }
            _queueStats.Stop();

            _stopped.Set();
            _queueMonitor.Unregister(this);
            Thread.EndThreadAffinity();
        }

        public void Publish(Message message)
        {
#if DEBUG
            _queueStats.Enqueued();
#endif
            _queue.Enqueue(message);
            if (_starving)
                _msgAddEvent.Set();
        }

        public void Handle(Message message)
        {
            Publish(message);
        }

        public QueueStats GetStatistics()
        {
            return _queueStats.GetStatistics(_queue.Count);
        }
    }
}