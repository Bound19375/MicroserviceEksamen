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
                r.AddConsumer<GrantLicenseConsumer>();
                r.AddConsumer<NotifyConsumer>();

                r.AddProducer<LicenseNotificationEvent>("Discord-License-Notification");

                r.AddSagaStateMachine<LicenseStateMachine, LicenseState>();

                r.UsingKafka((context, cfg) =>
                {

                    cfg.Host("kafka");

                    cfg.TopicEndpoint<LicenseNotificationEvent>("Discord-License-Notification", "Discord", e =>
                    {
                        e.CreateIfMissing(p => p.NumPartitions = 1);
                        e.AutoOffsetReset = AutoOffsetReset.Earliest;
                        e.ConfigureConsumer<GrantLicenseConsumer>(context);
                        e.UseMessageRetry(c => c.Interval(3, TimeSpan.FromSeconds(10)));
                        e.ConfigureConsumer<NotifyConsumer>(context);
                        e.UseMessageRetry(c => c.Interval(3, TimeSpan.FromSeconds(10)));
                        
                        e.ConfigureSaga<LicenseState>(context);
                    });
                });
            });
        });

        return services;
    }
}