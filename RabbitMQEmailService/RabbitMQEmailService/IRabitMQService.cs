using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQEmailService
{
    public interface IRabitMQService
    {
        public Task<string> SendEmailMessage<T>(T message, string routingKeyName, int ttl);

        Task<List<Email>> ReceiveEmailMessage<Email>(string routingKeyName);

        Task<string> SendFailEmailMessage<T>(T message, string routingKeyName, int ttl);

        Task<List<Email>> ReceiveSendFailEmailMessage<Email>(string routingKeyName);
    }
}
