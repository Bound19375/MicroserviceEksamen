using MassTransit;
using MassTransit.SerilogIntegration;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Logging.ClearProviders().AddSerilog().AddConsole();

//Kafka
builder.Services.AddMassTransit(x =>
{
    x.UsingInMemory();
    x.AddLogging();

    x.AddRider(r =>
    {
        r.UsingKafka((context, cfg) =>
        {
            cfg.Host("kafka");

            cfg.ClientId = "BackEnd";

            //cfg.TopmicEndPoint

            //
        });
    });

    //x.UsingRabbitMQ((context, cfg) =>
    //{
    //    cfg.Host("rabbitMQ", "/", h =>
    //    {
    //        h.Username(builder.Configuration["RabbitMQ:User"]);
    //        h.Password(builder.Configuration["RabbitMQ:Pass"]);
    //    });
    //    cfg.ConfigureEndpoints(context);
    //});
});

var app = builder.Build();

//app.MapGet("/", () => "Hello World!");

app.Run();
