using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messaging.Shared
{
    public interface IRabbitMqPublisher
    {
        void Publish<T>(T message, string exchange, string routingKey);
    }
}
