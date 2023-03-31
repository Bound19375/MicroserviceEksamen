using Auth.Database;
using Crosscutting.TransactionHandling;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Data;
using DiscordBot.Application.Interface;
using Crosscutting;
using Crosscutting.SellixPayload;
using DiscordSaga.Components.Discord;

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
        private readonly ILogger<LicenseStateMachine> _logger;
        private readonly IDiscordBotNotificationRepository _botNotificationRepository;
        private readonly IUnitOfWork<AuthDbContext> _unitOfWork;
        private readonly IDiscordGatewayBuyHandlerRepository _license;

        
        public State Granted { get; private set; }
        public Event<LicenseGrantEvent> LicenseGranted { get; private set; }
        public Event<LicenseNotificationEvent> Notify { get; private set; }

        public LicenseStateMachine(ILogger<LicenseStateMachine> logger, IDiscordBotNotificationRepository botNotificationRepository, IUnitOfWork<AuthDbContext> unitOfWork, IDiscordGatewayBuyHandlerRepository license)
        {
            _logger = logger;
            _botNotificationRepository = botNotificationRepository;
            _unitOfWork = unitOfWork;
            _license = license;

            Event(() => LicenseGranted, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => Notify, x => x.CorrelateById(context => context.Message.CorrelationId));

            InstanceState(x => x.CurrentState, Initial, Final, Granted);

            Initially(
                When(LicenseGranted)
                    .Then(context => _logger.LogInformation("Received GrantLicense command with payload: {Payload}", context.Message.Payload))
                    .ThenAsync(async context =>
                    {
                        await _unitOfWork.CreateTransaction(IsolationLevel.Serializable);
                        if (context.Message.Payload != null)
                        {
                            var licenseResponse = await _license.OrderHandler(context.Message.Payload);

                            await _unitOfWork.Commit();

                            context.Saga.Payload = licenseResponse.Payload;
                            context.Saga.Quantity = licenseResponse.Quantity;
                            context.Saga.Time = licenseResponse.Time;
                            context.Saga.WhichSpec = licenseResponse.WhichSpec;

                            await context.Publish(new LicenseNotificationEvent
                            {
                                Payload = licenseResponse.Payload,
                                Quantity = licenseResponse.Quantity,
                                Time = licenseResponse.Time,
                                WhichSpec = licenseResponse.WhichSpec,
                                CorrelationId = context.Data.CorrelationId,
                            });

                            _logger.LogInformation("Published Notify event with CorrelationId: {CorrelationId}", context.Message.CorrelationId);
                        }
                    })
                    .TransitionTo(Granted)
            );

            During(Granted,
                When(Notify)
                    .Then(context => _logger.LogInformation("Received Notify event with CorrelationId: {CorrelationId}", context.Message.CorrelationId))
                    .ThenAsync(async context =>
                    {
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
                    })
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }
    }

    public class DiscordPurchaseNotificationSaga : ISaga, InitiatedBy<LicenseGrantEvent>, Orchestrates<LicenseNotificationEvent>
    {
        public Guid CorrelationId { get; set; }

        private readonly ILogger<DiscordPurchaseNotificationSaga> _logger;
        private readonly IDiscordBotNotificationRepository _botNotificationRepository;
        private readonly IUnitOfWork<AuthDbContext> _unitOfWork;
        private readonly IDiscordGatewayBuyHandlerRepository _license;

#pragma warning disable CS8618
        public DiscordPurchaseNotificationSaga() { }
#pragma warning restore CS8618

        public DiscordPurchaseNotificationSaga(ILogger<DiscordPurchaseNotificationSaga> logger, IDiscordBotNotificationRepository botNotificationRepository, IUnitOfWork<AuthDbContext> unitOfWork, IDiscordGatewayBuyHandlerRepository license)
        {
            _logger = logger;
            _botNotificationRepository = botNotificationRepository;
            _unitOfWork = unitOfWork;
            _license = license;
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



