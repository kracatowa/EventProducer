using RabbitMQ.Client;

namespace EventProducer.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All); // Specify UTF-8    
            });
            builder.Services.Configure<RabbitMqServerSettings>(builder.Configuration.GetSection("RabbitMqServerSettings"));
            builder.Services.AddHealthChecks();

            var app = builder.Build();

            if (app.Environment.EnvironmentName.StartsWith("Local", StringComparison.InvariantCultureIgnoreCase))
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
   
            app.MapControllers();
            app.MapHealthChecks("/health");

            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            await TestRabbitMQConnection(app.Configuration, logger);

            app.Run();
        }

        private static async Task TestRabbitMQConnection(IConfiguration configuration, ILogger logger)
        {
            var rabbitMqServerSettings = configuration.GetSection("RabbitMqServerSettings").Get<RabbitMqServerSettings>();

            if (rabbitMqServerSettings == null)
            {
                logger.LogError("RabbitMqServerSettings are not configured properly.");
                return;
            }

            var factory = new ConnectionFactory()
            {
                HostName = rabbitMqServerSettings.Server,
                Port = rabbitMqServerSettings.ServerPort,
                UserName = rabbitMqServerSettings.Username,
                Password = rabbitMqServerSettings.Password
            };

            try
            {
                using var connection = await factory.CreateConnectionAsync();
                logger.LogInformation("Successfully connected to RabbitMQ server at {Server}.", rabbitMqServerSettings.Server);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to RabbitMQ server at {Server}.", rabbitMqServerSettings.Server);
            }
        }
    }
}
