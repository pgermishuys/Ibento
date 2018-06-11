using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ibento.DevelopmentHost.Messaging
{
    public class WriteLogbookEntryMessage : Message
    {
        private static readonly int TypeId = Interlocked.Increment(ref NextMsgId);
        public override int MsgTypeId => TypeId;
        public HttpContext Entity { get; }

        public WriteLogbookEntryMessage(HttpContext entity)
        {
            Entity = entity;
        }
    }
}
