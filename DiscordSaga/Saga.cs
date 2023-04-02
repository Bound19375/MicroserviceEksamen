using Auth.Database;
using Crosscutting.TransactionHandling;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Data;
using DiscordBot.Application.Interface;
using Crosscutting;
using Crosscutting.SellixPayload;
using DiscordSaga.Components.Discord;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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
        public Event<LicenseGrantEvent> GrantLicense { get; private set; }
        public Event<LicenseNotificationEvent> Notify { get; private set; }

        public LicenseStateMachine()
        {
            Event(() => GrantLicense, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => Notify, x => x.CorrelateById(context => context.Message.CorrelationId));

            InstanceState(x => x.CurrentState, Initial, Final, Granted);

            Initially(
                When(GrantLicense)
                    .Then(context =>
                    {
                        context.Saga.CorrelationId = context.Message.CorrelationId;
                    })
                    .TransitionTo(Granted)
            );

            During(Granted,
                When(Notify)
                    .Then(context =>
                    {
                        context.Saga.CorrelationId = context.Message.CorrelationId;
                        context.Saga.Quantity = context.Message.Quantity;
                        context.Saga.Time = context.Message.Time;
                        context.Saga.WhichSpec = context.Message.WhichSpec;
                    })
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }
    }

    public class LicenseGrantEventConsumer : IConsumer<LicenseGrantEvent>
    {
        private readonly ILogger<LicenseGrantEventConsumer> _logger;
        private readonly IUnitOfWork<AuthDbContext> _unitOfWork;
        private readonly IDiscordGatewayBuyHandlerRepository _license;
        private readonly ITopicProducer<LicenseNotificationEvent> _producer;

        public LicenseGrantEventConsumer(ILogger<LicenseGrantEventConsumer> logger, IUnitOfWork<AuthDbContext> unitOfWork, IDiscordGatewayBuyHandlerRepository license, ITopicProducer<LicenseNotificationEvent> producer)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _license = license;
            _producer = producer;
        }

        public async Task Consume(ConsumeContext<LicenseGrantEvent> context)
        {
            try
            {
                _logger.LogInformation("Received GrantLicense command with payload: {Payload}", context.Message.Payload);

                await _unitOfWork.CreateTransaction(IsolationLevel.Serializable);
                if (context.Message.Payload != null)
                {
                    var deserializePayload =
                        JsonConvert.DeserializeObject<SellixPayloadNormal.Root>(context.Message.Payload);

                    var license = await _license.OrderHandler(deserializePayload ?? throw new NullReferenceException("LicenseGrant Payload Null"));

                    await _unitOfWork.Commit();

                    await _producer.Produce(new LicenseNotificationEvent
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

    public class LicenseNotificationEventConsumer : IConsumer<LicenseNotificationEvent>
    {
        private readonly ILogger<LicenseNotificationEventConsumer> _logger;
        private readonly IDiscordBotNotificationRepository _botNotificationRepository;

        public LicenseNotificationEventConsumer(ILogger<LicenseNotificationEventConsumer> logger, IDiscordBotNotificationRepository botNotificationRepository)
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



