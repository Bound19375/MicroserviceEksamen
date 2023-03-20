using Confluent.Kafka;
using DiscordConsumers;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders().AddSerilog().AddConsole();

//Kafka
builder.Services.AddMassTransit(x =>
{
    //x.UsingInMemory();
    x.AddLogging();

    x.UsingRabbitMq((context, cfg) => {
        cfg.Host("rabbitMQ", "/", h => {
            h.Username(builder.Configuration["RabbitMQ:User"]);
            h.Password(builder.Configuration["RabbitMQ:Pass"]);
        });
        cfg.ConfigureEndpoints(context);
    });

    x.AddRider(r =>
    {
        r.AddConsumer<DiscordNotificationConsumer>();
        r.AddProducer<KafkaNotificationMessageDto>("Discord-Payment-Notification");

        r.UsingKafka((context, cfg) =>
        {
            cfg.ClientId = "BackEnd";

            cfg.Host("kafka");

            cfg.TopicEndpoint<KafkaNotificationMessageDto>("Discord-Payment-Notification", "Discord", e => {
                e.AutoOffsetReset = AutoOffsetReset.Earliest;
                e.ConfigureConsumer<DiscordNotificationConsumer>(context);
            });
        });
    });
});

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.Run();

