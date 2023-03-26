using Broker.MassTransitServiceCollection;
using DiscordBot.Application.Interface;
using DiscordBot.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders().AddSerilog().AddConsole();

builder.Services.AddScoped<IDiscordBotNotificationRepository, DiscordBotNotificationRepository>();
builder.Services.AddScoped<IDiscordGatewayBuyHandlerRepository, DiscordGatewayBuyHandlerRepository>();

//Kafka
builder.Services.AddMassTransitWithRabbitMqAndKafka(builder.Configuration);

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.Run();



