using Crosscutting.SellixPayload;
using Crosscutting;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DiscordNetConsumers;

namespace DiscordNetConsumers
{
    public record KafkaDiscordSagaMessageDto 
    {
        public SellixPayloadNormal.Root? Payload { get; init; }
        public int Quantity { get; init; }
        public DateTime Time { get; init; }
        public WhichSpec WhichSpec { get; init; }
        public DiscordTransportMessageState State { get; init; }
    }

    public enum DiscordTransportMessageState
    {
        Failed,
        LicenseGrantReady,
        NotificationReady
    }

    public class KafaDiscordSagaState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }

        public KafkaDiscordSagaMessageDto? Message { get; set; }
        public DiscordTransportMessageState State { get; set; }
    }


    public class KafkaDiscordSagaStateMachine : MassTransitStateMachine<KafaDiscordSagaState>
    {
        public State MessageProcessedState { get; private set; }
        public State NotificationReadyState { get; private set; }

        public Event<KafaDiscordSagaState> NotificationReadyEvent { get; private set; }
        public Event<KafaDiscordSagaState> LicenseGrantReadyEvent { get; private set; }

        public KafkaDiscordSagaStateMachine()
        {
            InstanceState(x => State(x.State.ToString()));

            Event(() => NotificationReadyEvent, x => x.CorrelateById(x => x.Message.CorrelationId));
            Event(() => LicenseGrantReadyEvent, x => x.CorrelateById(x => x.Message.CorrelationId));

            Initially(
                When(LicenseGrantReadyEvent)
                    .Then(context =>
                    {
                        context.Saga.Message = context.Message.Message;
                        context.Saga.State = DiscordTransportMessageState.LicenseGrantReady;
                    })
                    .TransitionTo(MessageProcessedState)
            );

            During(MessageProcessedState,
                When(NotificationReadyEvent)
                    .Then(context => context.Instance.State = DiscordTransportMessageState.NotificationReady)
                    .TransitionTo(NotificationReadyState)
            );

            //During(NotificationReadyState,
            //    // add code to execute when in the NotificationReady state
            //    // you can access the saga instance using 'context.Instance'
            //);

            //During(LicenseGrantReadyState,
            //    // add code to execute when in the LicenseGrantReady state
            //    // you can access the saga instance using 'context.Instance'
            //);

            SetCompletedWhenFinalized();
        }
    }
}




    public class DiscordNotificationConsumer : IConsumer<KafkaDiscordSagaMessageDto>
    {
        private readonly ILogger<DiscordNotificationConsumer> _logger;
        // private readonly IDiscordBotNotificationRepository _botNotification;

        public DiscordNotificationConsumer(IConfiguration configuration, ILogger<DiscordNotificationConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<KafkaDiscordSagaMessageDto> context)
        {
            if (context.Message.State == DiscordTransportMessageState.NotificationReady)
            {
                _logger.LogInformation("NotificationReady Queue Executed: " + context.Message);


            }
        }
    }
