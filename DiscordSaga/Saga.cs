﻿using Auth.Database;
using Crosscutting.TransactionHandling;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Data;
using DiscordBot.Application.Interface;
using Crosscutting;
using Crosscutting.SellixPayload;
using DiscordSaga.Components.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordSaga
{
    public class LicenseState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public SellixPayloadNormal.Root? Payload { get; set; }
        public int? Quantity { get; set; }
        public DateTime Time { get; set; }
        public WhichSpec? WhichSpec { get; set; }
        public int CurrentState { get; set; }
    }

    public class LicenseStateMachine : MassTransitStateMachine<LicenseState>
    {
        
        public State Granted { get; private set; }
        public Event<LicenseGrantEvent> LicenseGranted { get; private set; }
        public Event<LicenseNotificationEvent> Notify { get; private set; }

        public LicenseStateMachine()
        {
            Event(() => LicenseGranted, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => Notify, x => x.CorrelateById(context => context.Message.CorrelationId));

            InstanceState(x => x.CurrentState, Initial, Final, Granted);

            Initially(
                When(LicenseGranted)
                    .Then(context =>
                    {
                        context.Saga.Payload = context.Message.Payload;
                    })
                    .TransitionTo(Granted)
            );

            During(Granted,
                When(Notify)
                    .Then(context =>
                    {
                        context.Saga.Quantity = context.Message.Quantity;
                        context.Saga.Time = context.Message.Time;
                        context.Saga.WhichSpec = context.Message.WhichSpec;
                    })
                    .Publish(context => new LicenseNotificationEvent
                    {
                        Payload = context.Saga.Payload,
                        Quantity = context.Saga.Quantity,
                        Time = context.Saga.Time,
                        WhichSpec = context.Saga.WhichSpec,
                    })
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }
    }

    public class DiscordSagaConsumer : ISaga, InitiatedBy<LicenseGrantEvent>, Orchestrates<LicenseNotificationEvent>
    {
        public Guid CorrelationId { get; set; }

        public DiscordSagaConsumer() { }


        public async Task Consume(ConsumeContext<LicenseGrantEvent> context)
        {
            var _logger = context.GetPayload<ILogger<DiscordSagaConsumer>>();
            var _unitOfWork = context.GetPayload<IServiceProvider>().GetService<IUnitOfWork<AuthDbContext>>();
            var _license = context.GetPayload<IServiceProvider>().GetService<IDiscordGatewayBuyHandlerRepository>();

            try
            {
                _logger.LogInformation("Received GrantLicense command with payload: {Payload}", context.Message.Payload);

                await _unitOfWork.CreateTransaction(IsolationLevel.Serializable);
                if (context.Message.Payload != null)
                {
                    var license = await _license.OrderHandler(context.Message.Payload);

                    await _unitOfWork.Commit();

                    await context.Publish(new LicenseNotificationEvent
                    {
                        Payload = license.Payload,
                        Quantity = license.Quantity,
                        Time = license.Time,
                        WhichSpec = license.WhichSpec,
                        CorrelationId = context.Message.CorrelationId,
                    });

                    _logger.LogInformation("Published Notify event with CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "License Consumer Error");
            }
        }
        public async Task Consume(ConsumeContext<LicenseNotificationEvent> context)
        {
            var _logger = context.GetPayload<ILogger<DiscordSagaConsumer>>();
            var _botNotificationRepository = context.GetPayload<IServiceProvider>().GetService<IDiscordBotNotificationRepository>();
            var _unitOfWork = context.GetPayload<IServiceProvider>().GetService<IUnitOfWork<AuthDbContext>>();
            var _license = context.GetPayload<IServiceProvider>().GetService<IDiscordGatewayBuyHandlerRepository>();

            try
            {
                _logger.LogInformation("Received Notify event with CorrelationId: {CorrelationId}", context.Message.CorrelationId);

                // Perform some business logic to send the notification
                await _botNotificationRepository.NotificationHandler(new LicenseNotificationEvent
                {
                    Payload = context.Message.Payload,
                    Quantity = context.Message.Quantity,
                    Time = context.Message.Time,
                    WhichSpec = context.Message.WhichSpec
                });

                _logger.LogInformation("Sent notification with Quantity: {Quantity}, Time: {Time}, WhichSpec: {WhichSpec}",
                    context.Message.Quantity, context.Message.Time, context.Message.WhichSpec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification Error");
            }
        }
    }
}



