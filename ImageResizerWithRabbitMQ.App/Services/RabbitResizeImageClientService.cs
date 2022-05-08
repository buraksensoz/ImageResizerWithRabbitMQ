
using RabbitMQ.Client;
using System;

namespace ImageResizerWithRabbitMQ.App.Services
{
    public class RabbitResizeImageClientService : IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "ImageD_Exc";
        public static string RoutingName = "resize-route-image";
        public static string QueueName = "queue-resize-image";

        public RabbitResizeImageClientService(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IModel Connect() {
            try
            {
                _connection = _connectionFactory.CreateConnection();
            }
            catch (Exception)
            {

                return null;
            }
            
            if (_channel is {IsOpen:true })
            {
                return _channel;
            }

            _channel=_connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName,ExchangeType.Direct, true, false);
            _channel.QueueDeclare(QueueName, true, false, false, null);
            _channel.QueueBind(QueueName, ExchangeName, RoutingName);
            return _channel;
        
        
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
