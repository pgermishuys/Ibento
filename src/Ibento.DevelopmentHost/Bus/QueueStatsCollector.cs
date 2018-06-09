﻿using System;
using System.Diagnostics;
using System.Threading;
using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Bus
{
    public class QueueStatsCollector
    {
        private static readonly TimeSpan MinRefreshPeriod = TimeSpan.FromMilliseconds(100);

        public readonly string Name;
        public readonly string GroupName;

        public Type InProgressMessage { get { return _inProgressMsgType; } }

#if DEBUG        
        public static int NonIdle
        {
            get { return _nonIdle; }
        }
#endif
        private readonly object _statisticsLock = new object(); // this lock is mostly acquired from a single thread (+ rarely to get statistics), so performance penalty is not too high
        
        private readonly Stopwatch _busyWatch = new Stopwatch();
        private readonly Stopwatch _idleWatch = new Stopwatch();
        private readonly Stopwatch _totalIdleWatch = new Stopwatch();
        private readonly Stopwatch _totalBusyWatch = new Stopwatch();
        private readonly Stopwatch _totalTimeWatch = new Stopwatch();
        private TimeSpan _lastTotalIdleTime;
        private TimeSpan _lastTotalBusyTime;
        private TimeSpan _lastTotalTime;

        private long _totalItems;
        private long _lastTotalItems;
        private int _lifetimeQueueLengthPeak;
        private int _currentQueueLengthPeak;
        private Type _lastProcessedMsgType;
        private Type _inProgressMsgType;

        private bool _wasIdle;

        public QueueStatsCollector(string name, string groupName = null)
        {
            Name = name;
            GroupName = groupName;
        }

        public void Start()
        {
            _totalTimeWatch.Start();
#if DEBUG
            if (_notifyLock != null)
            {
                lock (_notifyLock)
                {
                    _nonIdle++;
                }
            }
#endif
            EnterIdle();
        }

        public void Stop()
        {
            EnterIdle();
            _totalTimeWatch.Stop();
        }

        public void ProcessingStarted<T>(int queueLength)
        {
            ProcessingStarted(typeof(T), queueLength);
        }

        public void ProcessingStarted(Type msgType, int queueLength)
        {
            _lifetimeQueueLengthPeak = _lifetimeQueueLengthPeak > queueLength ? _lifetimeQueueLengthPeak : queueLength;
            _currentQueueLengthPeak = _currentQueueLengthPeak > queueLength ? _currentQueueLengthPeak : queueLength;

            _inProgressMsgType = msgType;
        }

        public void ProcessingEnded(int itemsProcessed)
        {
            Interlocked.Add(ref _totalItems, itemsProcessed);
            _lastProcessedMsgType = _inProgressMsgType;
            _inProgressMsgType = null;
        }

        public void EnterIdle()
        {
            if (_wasIdle)
                return;
            _wasIdle = true;
#if DEBUG
            if (_notifyLock != null)
            {
                lock (_notifyLock)
                {
                    _nonIdle = NonIdle - 1;
                    if (NonIdle == 0)
                    {
                        Monitor.Pulse(_notifyLock);
                    }
                }
            }
#endif

            lock (_statisticsLock)
            {
                _totalIdleWatch.Start();
                _idleWatch.Restart();

                _totalBusyWatch.Stop();
                _busyWatch.Reset();
            }
        }

        public void EnterBusy()
        {
            if (!_wasIdle)
                return;
            _wasIdle = false;

#if DEBUG
            if (_notifyLock != null)
            {
                lock (_notifyLock)
                {
                    _nonIdle = NonIdle + 1;
                }
            }
#endif

            lock (_statisticsLock)
            {
                _totalIdleWatch.Stop();
                _idleWatch.Reset();

                _totalBusyWatch.Start();
                _busyWatch.Restart();
            }
        }

        public QueueStats GetStatistics(int currentQueueLength)
        {
            lock (_statisticsLock)
            {
                var totalTime = _totalTimeWatch.Elapsed;
                var totalIdleTime = _totalIdleWatch.Elapsed;
                var totalBusyTime = _totalBusyWatch.Elapsed;
                var totalItems = Interlocked.Read(ref _totalItems);

                var lastRunMs = totalTime - _lastTotalTime;
                var lastItems = totalItems - _lastTotalItems;
                var avgItemsPerSecond = lastRunMs.Ticks != 0 ? (int)(TimeSpan.TicksPerSecond * lastItems / lastRunMs.Ticks) : 0;
                var avgProcessingTime = lastItems != 0 ? (totalBusyTime - _lastTotalBusyTime).TotalMilliseconds / lastItems : 0;
                var idleTimePercent = Math.Min(100.0, lastRunMs.Ticks != 0 ? 100.0 * (totalIdleTime - _lastTotalIdleTime).Ticks / lastRunMs.Ticks : 100);

                var stats = new QueueStats(
                    Name,
                    GroupName,
                    currentQueueLength,
                    avgItemsPerSecond,
                    avgProcessingTime,
                    idleTimePercent,
                    _busyWatch.IsRunning ? _busyWatch.Elapsed : (TimeSpan?)null,
                    _idleWatch.IsRunning ? _idleWatch.Elapsed : (TimeSpan?)null,
                    totalItems,
                    _currentQueueLengthPeak,
                    _lifetimeQueueLengthPeak,
                    _lastProcessedMsgType,
                    _inProgressMsgType);

                if (totalTime - _lastTotalTime >= MinRefreshPeriod)
                {
                    _lastTotalTime = totalTime;
                    _lastTotalIdleTime = totalIdleTime;
                    _lastTotalBusyTime = totalBusyTime;
                    _lastTotalItems = totalItems;

                    _currentQueueLengthPeak = 0;
                }
                return stats;
            }
        }

#if DEBUG
        private static object _notifyLock;
        private static int _nonIdle = 0;
        private static int _length;
        public static bool DumpMessages;

        public static void InitializeIdleDetection(bool enable = true)
        {
            if (enable)
            {
                _nonIdle = 0;
                _length = 0;
                _notifyLock = new object();
            }
            else
            {
                _notifyLock = null;
            }
        }

#endif

        [Conditional("DEBUG")]
        public static void WaitIdle(bool waitForNonEmptyTf = false, int multiplier = 1)
        {
#if DEBUG
            var counter = 0;
            lock (_notifyLock)
            {
                var successes = 0;
                while (successes < 2)
                {
                    while (_nonIdle > 0 || _length > 0)
                    {
                        if (!Monitor.Wait(_notifyLock, 100))
                        {
                            Console.WriteLine("Waiting for IDLE state...");
                            counter++;
                            if (counter > 150 * multiplier)
                                throw new ApplicationException("Infinite loop?");
                        }
                    }
                    Thread.Sleep(10);
                    successes++;
                }
            }
#endif
        }

        [Conditional("DEBUG")]
        public void Enqueued()
        {
#if DEBUG
            Interlocked.Increment(ref _length);
#endif
        }

        [Conditional("DEBUG")]
        public void Dequeued(Message msg)
        {
#if DEBUG            
            Interlocked.Decrement(ref _length);
            if (DumpMessages)
            {
                Console.WriteLine(msg.GetType().Namespace + "." + msg.GetType().Name);
            }
#endif
        }
    } 
}