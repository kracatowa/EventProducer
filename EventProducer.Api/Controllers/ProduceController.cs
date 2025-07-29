using EventProducer.Api.Controllers.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace EventProducer.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProduceController(IOptions<ProducerSettings> producerSettings) : ControllerBase
    {
        public ProducerSettings ProducerSettings { get; } = producerSettings.Value;

        [HttpPost("send")]
        public async Task<IActionResult> SendMessageAsync([FromBody] SendMessageRequest request)
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = ProducerSettings.Server,
                    Port = ProducerSettings.ServerPort,
                    UserName = ProducerSettings.Username,
                    Password = ProducerSettings.Password
                };

                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(queue: request.Topic,
                                                durable: true,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);

                var body = Encoding.UTF8.GetBytes(request.Message);
                var messageId = Guid.NewGuid().ToString();
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"); // Human-readable timestamp  

                var basicProperties = new BasicProperties
                {
                    ContentType = "application/json",
                    ContentEncoding = "utf-8",
                    DeliveryMode = DeliveryModes.Persistent, // Persistent    
                    CorrelationId = Guid.NewGuid().ToString(),
                    MessageId = messageId,
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                    Headers = new Dictionary<string, object?>
                    {
                        { "source", request.Source },
                        { "subject", request.Subject },
                        { "localproducetime", timestamp }
                    }
                };

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: request.Topic,
                    mandatory: true,
                    basicProperties: basicProperties,
                    body: body
                );

                return Ok($"Message sent to RabbitMQ successfully with ID: {messageId} at {timestamp}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }

}
