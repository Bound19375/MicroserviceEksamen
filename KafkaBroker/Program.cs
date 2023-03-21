using Broker.MassTransitServiceCollection;
using Confluent.Kafka;
using DiscordNetConsumers;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders().AddSerilog().AddConsole();

//Kafka
builder.Services.AddMassTransitWithRabbitMqAndKafka(builder.Configuration);

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.Run();

