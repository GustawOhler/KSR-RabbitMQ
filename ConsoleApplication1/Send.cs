using System;
using RabbitMQ.Client;
using System.Text;
using System.Collections.Generic;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;

namespace ConsoleApplication1
{
    class Send
    {

        public static void pierwszaCzesc()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                string replyQueueName = "helloreply";
                replyQueueName = channel.QueueDeclare().QueueName;
                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                IBasicProperties properties = channel.CreateBasicProperties();
                var correlationId = Guid.NewGuid().ToString();
                properties.CorrelationId = correlationId;
                properties.ReplyTo = replyQueueName;
                BlockingCollection<string> respQueue = new BlockingCollection<string>();

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var response = Encoding.UTF8.GetString(body);
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        respQueue.Add(response);
                    }
                };

                properties.Headers = new Dictionary<string, object>();

                for (int i = 0; i < 10; i++)
                {
                    if (i == 0)
                        properties.Headers.Add("nr wiadomosci", i);
                    else
                    {
                        properties.Headers.Remove("nr wiadomosci");
                        properties.Headers.Add("nr wiadomosci", i);
                    }

                    string message = "Hello World!";
                    var body = Encoding.UTF8.GetBytes(message);

                    Console.WriteLine(" [x] Sent {0}", message);
                    channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: properties, body: body);
                    channel.BasicConsume(consumer, replyQueueName, true);
                    string resp = respQueue.Take();
                    if (resp.Length > 0)
                        Console.WriteLine("{0}", resp);
                    else
                        Console.WriteLine("No response");
                }
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }

        public static void drugaCzesc()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic_logs",
                                        type: "topic");
                string message = "";
                var routingKey = "";
                for (int i = 0; i < 10; i++)
                {
                    if (i % 2 == 0)
                        routingKey = "abc.def";
                    else
                        routingKey = "abc.xyz";
                    message = "Hello world nr " + i;
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "topic_logs",
                                         routingKey: routingKey,
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, message);
                }
            }
        }
        public static void Main()
        {
            pierwszaCzesc();
            //drugaCzesc();
        }
    }
}