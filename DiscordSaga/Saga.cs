using Auth.Database;
using Crosscutting.TransactionHandling;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Data;
using Crosscutting.KafkaDto.Discord;
using DiscordBot.Application.Interface;
using Crosscutting.SellixPayload;
using Crosscutting;

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

            InstanceState(x => x.CurrentState, Initial, Granted, Final);

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

    public class GrantLicenseConsumer : IConsumer<LicenseGrantEvent>
    {
        private readonly ILogger<GrantLicenseConsumer> _logger;
        private readonly IUnitOfWork<AuthDbContext> _unitOfWork;
        private readonly IDiscordGatewayBuyHandlerRepository _license;
        public GrantLicenseConsumer(ILogger<GrantLicenseConsumer> logger, IDiscordGatewayBuyHandlerRepository license, IUnitOfWork<AuthDbContext> unitOfWork)
        {
            _logger = logger;
            _license = license;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<LicenseGrantEvent> context)
        {
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
    }

    public class NotifyConsumer : IConsumer<LicenseNotificationEvent>
    {
        private readonly ILogger<NotifyConsumer> _logger;
        private readonly IDiscordBotNotificationRepository _botNotificationRepository;

        public NotifyConsumer(ILogger<NotifyConsumer> logger, IDiscordBotNotificationRepository botNotificationRepository)
        {
            _logger = logger;
            _botNotificationRepository = botNotificationRepository;
        }

        public async Task Consume(ConsumeContext<LicenseNotificationEvent> context)
        {
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



