using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ibento.DevelopmentHost.Messaging;
using Serilog;

namespace Ibento.DevelopmentHost.Bus
{
    public class InMemoryBus : IBus, IHandle<Message>
    {
        public static InMemoryBus CreateTest()
        {
            return new InMemoryBus();
        }

        public static readonly TimeSpan DefaultSlowMessageThreshold = TimeSpan.FromMilliseconds(48);

        public string Name { get; private set; }

        private readonly List<IMessageHandler>[] _handlers;

        private readonly bool _watchSlowMsg;
        private readonly TimeSpan _slowMsgThreshold;

        private InMemoryBus(): this("Test"){
        }

        public InMemoryBus(string name, bool watchSlowMsg = true, TimeSpan? slowMsgThreshold = null)
        {
            Name = name;
            _watchSlowMsg = watchSlowMsg;
            _slowMsgThreshold = slowMsgThreshold ?? DefaultSlowMessageThreshold;

            _handlers = new List<IMessageHandler>[MessageHierarchy.MaxMsgTypeId + 1];
            for (int i = 0; i < _handlers.Length; ++i)
            {
                _handlers[i] = new List<IMessageHandler>();
            }
        }

        public void Subscribe<T>(IHandle<T> handler) where T : Message
        {
            int[] descendants = MessageHierarchy.DescendantsByType[typeof (T)];
            for (int i = 0; i < descendants.Length; ++i)
            {
                var handlers = _handlers[descendants[i]];
                if (!handlers.Any(x => x.IsSame<T>(handler)))
                    handlers.Add(new MessageHandler<T>(handler, handler.GetType().Name));
            }
        }

        public void Unsubscribe<T>(IHandle<T> handler) where T : Message
        {
            int[] descendants = MessageHierarchy.DescendantsByType[typeof(T)];
            for (int i = 0; i < descendants.Length; ++i)
            {
                var handlers = _handlers[descendants[i]];
                var messageHandler = handlers.FirstOrDefault(x => x.IsSame<T>(handler));
                if (messageHandler != null)
                    handlers.Remove(messageHandler);
            }
        }

        public async Task Handle(Message message)
        {
            await Publish(message);
        }

        public async Task Publish(Message message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var handlers = _handlers[message.MsgTypeId];
            for (int i = 0, n = handlers.Count; i < n; ++i)
            {
                var handler = handlers[i];
                if (_watchSlowMsg)
                {
                    var start = DateTime.UtcNow;

                    await handler.TryHandle(message);

                    var elapsed = DateTime.UtcNow - start;
                    Log.Information("Executed {Message} in {Time}ms", message.GetType().Name, elapsed.TotalMilliseconds);
                    if (elapsed > _slowMsgThreshold)
                    {
                        Log.Debug("SLOW BUS MSG [{0}]: {1} - {2}ms. Handler: {3}.",
                                  Name, message.GetType().Name, (int)elapsed.TotalMilliseconds, handler.HandlerName);
                    }
                }
                else
                {
                    await handler.TryHandle(message);
                }
            }
        }
    }
}