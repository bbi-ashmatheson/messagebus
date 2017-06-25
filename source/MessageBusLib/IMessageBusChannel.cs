using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageBus
{
    public interface IMessageBusChannel
    {
        string Name { get; }

        void DoCommand(MessageData message);
    }
}
