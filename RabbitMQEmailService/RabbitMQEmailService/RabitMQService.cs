using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMQEmailService
{
    public class RabitMQService : IRabitMQService
    {
        public async Task<string> SendEmailMessage<T>(T message, string routingKeyName, int ttl)
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())


                using (var channel = connection.CreateModel())
                {

                    //a “Email“ to a RabbitMQ FIFO Queue.A TTL should be set(24 hours).
                    var args = new Dictionary<string, object>();
                    args.Add("x-message-ttl", 86400000);


                    channel.QueueDeclare(queue: routingKeyName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: args);


                    string messagetemp = await JsonHelper.SerializeAsync<T>(message);
                    var body = Encoding.UTF8.GetBytes(messagetemp);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: "",
                                         routingKey: routingKeyName,
                                         basicProperties: properties,
                                         body: body);
                    Console.WriteLine(" [x] Sent {0}", message);

                    return messagetemp;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }



        }


        public async Task<List<Email>> ReceiveEmailMessage<Email>(string routingKeyName)
        {
            try
            {
                List<Email> EmailsList = new List<Email>();
                List<string> stringList = new List<string>();

                var factory = new ConnectionFactory() { HostName = "localhost" };

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        //a “Email“ to a RabbitMQ FIFO Queue.A TTL should be set(24 hours).
                        var args = new Dictionary<string, object>();
                        args.Add("x-message-ttl", 86400000);
                        channel.QueueDeclare(queue: routingKeyName,
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: args);
                        channel.BasicQos(prefetchSize: 0, prefetchCount: 100, global: false);

                        var consumer = new EventingBasicConsumer(channel);
                        channel.BasicConsume(queue: routingKeyName, autoAck: true, consumer: consumer);

                        consumer.Received += async (model, ea) =>
                        {
                            try
                            {
                                var body = ea.Body.ToArray();
                                var message = Encoding.UTF8.GetString(body);
                                //If not sucessfull , use DLX to delay requeing of the job for later (60 seconds)

                                // Insert into List
                                stringList.Add(message);
                            }
                            catch (Exception e)
                            {
                                throw new Exception(e.Message);

                            }
                        };
                        Thread.Sleep(1000);

                        //   channel.BasicConsume(queue: routingKeyName,
                        //autoAck: true,
                        //consumer: consumer);
                    }

                }
                //Console.WriteLine(stringList.Count);
                foreach (var item in stringList)
                {
                    EmailsList.Add(JsonHelper.DeserializeAsync<Email>(item).Result);
                }

                return EmailsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" [x] error {0}", ex.Message);
                return new List<Email>();
            }
        }

        //use DLX to delay requeing of the job for later (60 seconds)

        public async Task<string> SendFailEmailMessage<T>(T message, string routingKeyName, int ttl)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())


            using (var channel = connection.CreateModel())
            {

                //a “Email“ to a RabbitMQ FIFO Queue.A TTL should be set(24 hours).
                var args = new Dictionary<string, object>();
                args.Add("x-message-ttl", ttl);
                args.Add("x-dead-letter-exchange", "FailedEmailExchange");
                args.Add("x-dead-letter-routing-key", "FailedEmailExchange-routing-key");
                channel.ExchangeDeclare("FailedExchangeEmail", "direct");

                channel.QueueDeclare(queue: routingKeyName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: args);


                string messagetemp = await JsonHelper.SerializeAsync<T>(message);
                var body = Encoding.UTF8.GetBytes(messagetemp);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                     routingKey: routingKeyName,
                                     basicProperties: properties,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);

                return messagetemp;
            }



        }

        //used DLX to delay requeing of the job for later (60 seconds)
        public async Task<List<Email>> ReceiveSendFailEmailMessage<Email>(string routingKeyName)
        {
            try
            {
                List<Email> EmailsList = new List<Email>();
                List<string> stringList = new List<string>();

                var factory = new ConnectionFactory() { HostName = "localhost" };

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        //a “Email“ to a RabbitMQ FIFO Queue.A TTL should be set(24 hours).

                        var args = new Dictionary<string, object>();
                        args.Add("x-message-ttl", 86400000);
                        args.Add("x-dead-letter-exchange", "FailedEmailExchange");
                        args.Add("x-dead-letter-routing-key", "FailedEmailExchange-routing-key");
                        channel.ExchangeDeclare("FailedExchangeEmail", "direct");
                        channel.QueueDeclare(queue: routingKeyName,
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: args);
                        channel.BasicQos(prefetchSize: 0, prefetchCount: 100, global: false);

                        var consumer = new EventingBasicConsumer(channel);
                       channel.BasicConsume(queue: routingKeyName, autoAck: false, consumer: consumer);

                        consumer.Received += async (model, ea) =>
                        {
                            try
                            {
                                var body = ea.Body.ToArray();
                                var message = Encoding.UTF8.GetString(body);
                                //If not sucessfull , use DLX to delay requeing of the job for later (60 seconds)

                                // Insert into List
                                stringList.Add(message);
                            }
                            catch (Exception e)
                            {
                                throw new Exception(e.Message);

                            }
                        };
                        Thread.Sleep(1000);

                        //   channel.BasicConsume(queue: routingKeyName,
                        //autoAck: true,
                        //consumer: consumer);
                    }

                }
                //Console.WriteLine(stringList.Count);
                foreach (var item in stringList)
                {
                    var res = await JsonHelper.DeserializeAsync<Email>(item);
                    EmailsList.Add(res);
                }

                return EmailsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" [x] error {0}", ex.Message);
                return new List<Email>();
            }
        }


    }
}
