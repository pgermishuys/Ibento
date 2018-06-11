using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ibento.DevelopmentHost.Bus;
using Ibento.DevelopmentHost.Messaging;

namespace Ibento.DevelopmentHost.Services
{
    public class LogbookEntryWriter : IHandle<WriteLogbookEntryMessage>
    {
        private IPublisher _mainBus;

        public LogbookEntryWriter(IPublisher mainBus)
        {
            _mainBus = mainBus;
        }
        public void Handle(WriteLogbookEntryMessage message)
        {
            message.Entity.Response.StatusCode = 200;
            var bytes = Encoding.UTF8.GetBytes("Success");
            message.Entity.Response.Body.Write(bytes, 0, bytes.Length);
            message.Entity.Response.Body.Flush();
        }
    }
}
