﻿using Auth.Database;
using Confluent.Kafka;
using Crosscutting.TransactionHandling;
using DiscordBot.Application.Interface;
using DiscordBot.Infrastructure;
using DiscordSaga;
using DiscordSaga.Components.Discord;
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
                r.AddSagaStateMachine<LicenseStateMachine, LicenseState>().InMemoryRepository(); //MongoDb
                r.AddSaga<DiscordSagaConsumer>().InMemoryRepository(); //MongoDb

                r.AddProducer<LicenseGrantEvent>("Discord-License");

                r.UsingKafka((context, cfg) =>
                {
                    cfg.Host("kafka");

                    cfg.TopicEndpoint<Null,LicenseGrantEvent>("Discord-License", "Discord", e =>
                    {
                        e.CreateIfMissing(p => p.NumPartitions = 1);
                        e.AutoOffsetReset = AutoOffsetReset.Earliest;
                        e.ConfigureSaga<DiscordSagaConsumer>(context);
                    });

                    cfg.TopicEndpoint<Null, LicenseNotificationEvent>("Discord-Notification", "Discord", e =>
                    {
                        e.CreateIfMissing(p => p.NumPartitions = 1);
                        e.AutoOffsetReset = AutoOffsetReset.Earliest;
                        e.ConfigureSaga<DiscordSagaConsumer>(context);
                    });
                });
            });
        });

        return services;
    }
}