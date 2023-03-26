using Auth.Database;
using Crosscutting.TransactionHandling;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Data;
using Crosscutting.KafkaDto.Discord;
using DiscordBot.Application.Interface;

namespace DiscordSaga
{
    public class KafkaDiscordSagaState : SagaStateMachineInstance, CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public KafkaDiscordSagaMessageDto? Message { get; set; }
        public SagaState State { get; set; } 
    }

    public enum SagaState
    {
        LicenseReady,
        NotificationReady,
    }

    public class KafkaDiscordSagaStateMachine : MassTransitStateMachine<KafkaDiscordSagaState>
    {
        public State? LicenseReady { get; private set; }
        public State? NotificationReady { get; private set; }

        public Event<KafkaDiscordSagaState>? NotificationReadyEvent { get; private set; }
        public Event<KafkaDiscordSagaState>? LicenseGrantReadyEvent { get; private set; }

        public KafkaDiscordSagaStateMachine()
        {
            InstanceState(x => (int)x.State, LicenseReady, NotificationReady);

            Event(() => NotificationReadyEvent, x => x.CorrelateById(context => context.Message.CorrelationId));
            Event(() => LicenseGrantReadyEvent, x => x.CorrelateById(context => context.Message.CorrelationId));

            Initially(
                When(LicenseGrantReadyEvent)
                    .Then(context =>
                    {
                        context.Saga.Message = context.Message.Message;
                        context.Saga.State = SagaState.LicenseReady;
                    })
                    //RabbitMq
                    .Publish(context => new KafkaDiscordSagaState
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        Message = context.Saga.Message,
                        State = context.Saga.State = SagaState.LicenseReady
                    })
                    //Kafka
                    .Produce(context => context.Init<KafkaDiscordSagaState>(new
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        Message = context.Saga.Message,
                        State = context.Saga.State = SagaState.LicenseReady
                    }))
                    .TransitionTo(NotificationReady)
            );

            During(NotificationReady,
                When(NotificationReadyEvent)
                    .Then(context =>
                    {
                        context.Saga.Message = context.Saga.Message;
                        context.Saga.State = SagaState.NotificationReady;
                    })
                    .Finalize()
            );

            SetCompletedWhenFinalized();
        }
    }

    public class DiscordSagaConsumer : ISaga, InitiatedByOrOrchestrates<KafkaDiscordSagaState>
    {
        private readonly ILogger<DiscordSagaConsumer> _logger;
        private readonly IDiscordBotNotificationRepository _botNotification;
        private readonly IUnitOfWork<AuthDbContext> _authDbContext;
        private readonly IDiscordGatewayBuyHandlerRepository _buyHandler;

        public DiscordSagaConsumer(ILogger<DiscordSagaConsumer> logger, IDiscordBotNotificationRepository botNotification, IUnitOfWork<AuthDbContext> authDbContext, IDiscordGatewayBuyHandlerRepository buyHandler)
        {
            _logger = logger;
            _botNotification = botNotification;
            _authDbContext = authDbContext;
            _buyHandler = buyHandler;
        }

        public Guid CorrelationId { get; set; }

        public async Task Consume(ConsumeContext<KafkaDiscordSagaState> context)
        {
            try
            {
                switch (context.Message.State)
                {
                    case SagaState.LicenseReady:
                        try
                        {
                            _logger.LogInformation("LicenseGrant Consumer Executed: " + context.Message);

                            if (context.Message.Message != null)
                            {
                                if (context.Message.Message.Payload != null)
                                {
                                    await _authDbContext.CreateTransaction(IsolationLevel.Serializable);
                                    await _buyHandler.OrderHandler(context.Message.Message.Payload);
                                }
                            }

                            await _authDbContext.Commit();
                        }
                        catch (Exception ex)
                        {
                            await _authDbContext.Rollback();
                            _logger.LogError(ex.Message);
                        }
                        break;

                    case SagaState.NotificationReady:
                        _logger.LogInformation("NotificationReady Consumer Executed: " + context.Message);
                        if (context.Message.Message != null)
                            await _botNotification.NotificationHandler(context.Message.Message);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}



