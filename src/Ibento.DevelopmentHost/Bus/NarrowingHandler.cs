﻿using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Bus
{
    public class NarrowingHandler<TInput, TOutput> : IHandle<TInput>
        where TInput : Message
        where TOutput : TInput
    {
        private readonly IHandle<TOutput> _handler;

        public NarrowingHandler(IHandle<TOutput> handler)
        {
            _handler = handler;
        }

        public void Handle(TInput message)
        {
            _handler.Handle((TOutput) message); // will throw if message type is wrong
        }
    }
}