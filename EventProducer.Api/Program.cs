using RabbitMQ.Client;

namespace EventProducer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add controllers to the service collection  
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All); // Specify UTF-8  
            });

            // Bind ProducerSettings to IOptions  
            builder.Services.Configure<ProducerSettings>(builder.Configuration.GetSection("ProducerSettings"));

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Map controllers  
            app.MapControllers();

            // Add RabbitMQ connection test  
            var producerSettings = builder.Configuration.GetSection("ProducerSettings").Get<ProducerSettings>();

            if (producerSettings == null)
            {
                Console.WriteLine("ProducerSettings are not configured properly.");
                return;
            }

            var factory = new ConnectionFactory()
            {
                HostName = producerSettings.Server,
                Port = producerSettings.ServerPort,
                UserName = producerSettings.Username,
                Password = producerSettings.Password
            };
            try
            {
                using var connection = await factory.CreateConnectionAsync();
                Console.WriteLine($"Successfully connected to RabbitMQ server at {producerSettings.Server}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to RabbitMQ server at {producerSettings.Server}: {ex.Message}");
            }

            app.Run();
        }
    }
}
