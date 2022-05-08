using ImageResizerWithRabbitMQ.App.Services;
using ImageResizerWithRabbitMQ.App.Services.Event;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizerWithRabbitMQ.App.BackgroundServices
{
    public class ImageResizeProcessService : BackgroundService
    {

        private readonly RabbitResizeImageClientService _rabbitResizeImageClientService;
        private IModel _channel;
        

        public ImageResizeProcessService(RabbitResizeImageClientService rabbitResizeImageClientService)
        {
            _rabbitResizeImageClientService = rabbitResizeImageClientService;
            
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitResizeImageClientService.Connect();

            _channel?.BasicQos(0, 1, false);
            return base.StartAsync(cancellationToken);
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel==null) return Task.CompletedTask;
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(RabbitResizeImageClientService.QueueName, false, consumer);

            consumer.Received += Consumer_Received;


            return Task.CompletedTask;
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                var imageEvent = JsonSerializer.Deserialize<ImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));
                var imageF = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\images", imageEvent.ImageFileName);
               if (!File.Exists(imageF))
                {
                    _channel.BasicAck(@event.DeliveryTag, false);
                    return Task.CompletedTask;

                }

                
                ResizeImage(imageEvent, imageF, 25);
                ResizeImage(imageEvent, imageF, 50);
                ResizeImage(imageEvent, imageF, 100);
                ResizeImage(imageEvent, imageF, 200);
                _channel.BasicAck(@event.DeliveryTag, false);
                return Task.CompletedTask;
            }
            catch (System.Exception)
            {
                return Task.CompletedTask;

            }
        }

        private static void ResizeImage(ImageCreatedEvent imageEvent, string imageF, int size)
        {
            using (var image = Image.Load(imageF))
            {
                image.Mutate(x => x.Resize(size, size));
                var savepath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/" + size, imageEvent.ImageFileName);
                image.Save(savepath);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }


    }
}
