using Confluent.Kafka;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders().AddSerilog().AddConsole();

//Kafka
builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory();
    x.AddLogging();

    //x.UsingRabbitMq((context, cfg) => {
    //    cfg.Host("rabbitMQ", "/", h => {
    //        h.Username(builder.Configuration["RabbitMQ:User"]);
    //        h.Password(builder.Configuration["RabbitMQ:Pass"]);
    //    });
    //    cfg.ConfigureEndpoints(context);
    //});

    x.AddRider(r =>
    {
        //r.AddProducer();
        //r.AddConsumer();

        r.UsingKafka((context, cfg) =>
        {
            cfg.ClientId = "BackEnd";

            cfg.Host("broker");

            //cfg.TopicEndpoint("my-topic", "my-group-id", e => {
            //    e.AutoOffsetReset = AutoOffsetReset.Earliest;
            //    e.ConfigureConsumer<MyConsumer>(context);
            //});
        });
    });
});

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.Run();
