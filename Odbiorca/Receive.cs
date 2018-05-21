using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Odbiorca
{
    class Receive
    {
        public static void pierwszaCzesc()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: "hello", autoAck: false, consumer: consumer);
                Console.WriteLine(" [x] Awaiting RPC requests");
                consumer.Received += (model, ea) =>
                {
                    string response = null;
                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;
                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        int messageNumber = (int)ea.BasicProperties.Headers["nr wiadomosci"];
                        messageNumber++;
                        Console.WriteLine(" [x] Received {0} nr of message: {1}", message, messageNumber);
                        response = "Reply from message number: " + messageNumber.ToString();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(" [.] " + e.Message);
                        response = "";
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }

                    Thread.Sleep(1000);
                };
                //channel.BasicConsume(queue: "hello", autoAck: false, consumer: consumer);


                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        public static void drugaCzesc()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic_logs", type: "topic");
                var queueName = channel.QueueDeclare().QueueName;

                string bindingKey = "abc.*";

                    channel.QueueBind(queue: queueName,
                                      exchange: "topic_logs",
                                      routingKey: bindingKey);

                Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [x] Received '{0}':'{1}'",
                                      routingKey,
                                      message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
        public static void Main()
        {
            pierwszaCzesc();
            //drugaCzesc();
        }
    }
}