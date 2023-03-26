using Confluent.Kafka;
using Crosscutting.KafkaDto.Discord;
using DiscordSaga;
using MassTransit;
using Serilog;

namespace Broker.MassTransitServiceCollection;

public static class MassTransitConfiguration
{
    public static IServiceCollection AddMassTransitWithRabbitMqAndKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddLogging(l =>
            {
                l.ClearProviders();
                l.AddSerilog();
                l.AddConsole();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("rabbitMQ", "/", h =>
                {
                    h.Username(configuration["RabbitMQ:User"]);
                    h.Password(configuration["RabbitMQ:Pass"]);
                });
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(r =>
            {
                r.AddConsumer<DiscordSagaConsumer>();

                r.AddProducer<KafkaDiscordSagaMessageDto>("Discord-License-Notification");

                r.AddSagaStateMachine<KafkaDiscordSagaStateMachine, KafkaDiscordSagaState>();

                r.UsingKafka((context, cfg) =>
                {

                    cfg.Host("kafka");

                    cfg.TopicEndpoint<KafkaDiscordSagaMessageDto>("Discord-License-Notification", "Discord", e =>
                    {
                        e.CreateIfMissing(p => p.NumPartitions = 1);
                        e.AutoOffsetReset = AutoOffsetReset.Earliest;
                        e.ConfigureConsumer<DiscordSagaConsumer>(context);
                        e.UseMessageRetry(c => c.Interval(3, TimeSpan.FromSeconds(10)));

                        e.ConfigureSaga<KafkaDiscordSagaState>(context);

                    });
                });
            });
        });

        return services;
    }
}