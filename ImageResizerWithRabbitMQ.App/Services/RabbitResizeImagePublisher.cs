using ImageResizerWithRabbitMQ.App.Services.Event;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ImageResizerWithRabbitMQ.App.Services
{
    public class RabbitResizeImagePublisher
    {
        private readonly RabbitResizeImageClientService _client;
        private IModel _channel;
        public RabbitResizeImagePublisher(RabbitResizeImageClientService client)
        {
            _client = client;
        }

        public bool Publish(ImageCreatedEvent imageCreatedEvent) {
            
                _channel = _client.Connect();
            
            if (_channel==null)
                return false;
            
            
            var body=Encoding.UTF8.GetBytes(JsonSerializer.Serialize(imageCreatedEvent));
            var properties= _channel.CreateBasicProperties();
            properties.Persistent= true;
            _channel.BasicPublish(
                RabbitResizeImageClientService.ExchangeName,
                RabbitResizeImageClientService.RoutingName, 
                false,
                basicProperties: properties, 
                body: body);

            return true;
        
        
        
        }



    }
}
