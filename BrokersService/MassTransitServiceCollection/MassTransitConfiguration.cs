using Auth.Database;
using Confluent.Kafka;
using Crosscutting.TransactionHandling;
using DiscordBot.Application.Interface;
using DiscordBot.Infrastructure;
using DiscordSaga;
using DiscordSaga.Components.Events;
using DiscordSaga.Consumers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

namespace BrokersService.MassTransitServiceCollection;

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

            x.TryAddScoped<IUnitOfWork<AuthDbContext>, UnitOfWork<AuthDbContext>>();
            x.TryAddScoped<IDiscordGatewayBuyHandlerRepository, DiscordGatewayBuyHandlerRepository>();
            x.TryAddScoped<IDiscordBotNotificationRepository, DiscordBotNotificationRepository>();

            x.AddRider(r =>
            {
                r.AddSagaStateMachine<LicenseStateMachine, LicenseState>().MongoDbRepository(m =>
                {
                    m.Connection =
                        $"mongodb://{configuration["MONGO:USER"]}:{configuration["MONGO:PASSWORD"]}@mongoDB:27017";//"/?authMechanism=DEFAULT";
                    m.DatabaseName = "licensedb";
                    m.CollectionName = "license";
                });

                r.AddProducer<LicenseGrantEvent>("Discord-License-Grant");
                r.AddProducer<LicenseNotificationEvent>("Discord-Notification");

                r.AddConsumer<LicenseGrantEventConsumer>();
                r.AddConsumer<LicenseNotificationEventConsumer>();

                r.UsingKafka((context, cfg) =>
                {
                    cfg.Host("kafka");

                    cfg.TopicEndpoint<Null,LicenseGrantEvent>("Discord-License-Grant", "Discord", e =>
                    {
                        e.CreateIfMissing(m =>
                        {
                            m.NumPartitions = 1;
                        });
                        e.ConfigureSaga<LicenseState>(context);
                        e.UseNewtonsoftJsonSerializer();
                        e.UseNewtonsoftJsonDeserializer();
                        e.ConfigureConsumer<LicenseGrantEventConsumer>(context);
                    });

                    cfg.TopicEndpoint<Null, LicenseNotificationEvent>("Discord-Notification", "Discord", e =>
                    {
                        e.CreateIfMissing(m =>
                        {
                            m.NumPartitions = 1;
                        });
                        e.ConfigureSaga<LicenseState>(context);
                        e.UseNewtonsoftJsonSerializer();
                        e.UseNewtonsoftJsonDeserializer();
                        e.ConfigureConsumer<LicenseNotificationEventConsumer>(context); 
                    });
                });
            });
        });

        return services;
    }
}