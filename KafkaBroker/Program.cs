using Broker.MassTransitServiceCollection;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders().AddSerilog().AddConsole();
    
//builder.Services.AddScoped<IDiscordBotNotificationRepository, DiscordBotRepository>();

//Kafka
builder.Services.AddMassTransitWithRabbitMqAndKafka(builder.Configuration);

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.Run();

