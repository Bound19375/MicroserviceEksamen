using Confluent.Kafka;
using DiscordNetConsumers;
using MassTransit;
using Serilog;

namespace Broker.MassTransitServiceCollection;

public static class MassTransitConfiguration {
    public static IServiceCollection AddMassTransitWithRabbitMqAndKafka(this IServiceCollection services, IConfiguration configuration) 
    {
        services.AddMassTransit(x => 
        {
            x.AddLogging(l => {
                l.ClearProviders();
                l.AddSerilog();
                l.AddConsole();
            });

            x.UsingRabbitMq((context, cfg) => {
                cfg.Host("rabbitMQ", "/", h => {
                    h.Username(configuration["RabbitMQ:User"]);
                    h.Password(configuration["RabbitMQ:Pass"]);
                });
                cfg.ConfigureEndpoints(context);
            });

            x.AddRider(r => {
                r.AddConsumer<DiscordNotificationConsumer>();
                r.AddProducer<KafkaDiscordSagaMessageDto>("Discord-Payment-Notification");

                r.UsingKafka((context, cfg) => {
                    cfg.ClientId = "Api.Discord";

                    cfg.Host("kafka");

                    cfg.TopicEndpoint<KafkaDiscordSagaMessageDto>("Discord-Payment-Notification", "Discord", e => {
                        e.CreateIfMissing(p => p.NumPartitions = 1);
                        e.AutoOffsetReset = AutoOffsetReset.Earliest;
                        e.ConfigureConsumer<DiscordNotificationConsumer>(context);
                    });
                });
            });
        });

        return services;
    }
}