namespace EventProducer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add controllers to the service collection  
            builder.Services.AddControllers();

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

            app.Run();
        }
    }
}
