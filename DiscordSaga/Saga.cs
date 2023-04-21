using MassTransit;
using Crosscutting;
using DiscordSaga.Components.Events;

namespace DiscordSaga;

public class LicenseState : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public string? Payload { get; set; }
    public int? Quantity { get; set; }
    public DateTime Time { get; set; }
    public WhichSpec? WhichSpec { get; set; }
    public int CurrentState { get; set; }
    public int Version { get; set; }
}

public class LicenseStateMachine : MassTransitStateMachine<LicenseState>
{
    public State Granted { get; set; }
    public State NotificationReady { get; set; }
    public Event<LicenseGrantEvent> GrantLicense { get; set; }
    public readonly TaskCompletionSource<bool> GrantLicenseEventProcessed = new();
    public Event<LicenseNotificationEvent> Notify { get; set; }


    public LicenseStateMachine()
    {
        Event(() => GrantLicense, x  => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => Notify, x => x.CorrelateById(context => context.Message.CorrelationId));

        InstanceState(x => x.CurrentState, Initial, Final, Granted, NotificationReady);

        Initially(
            When(GrantLicense)
                .ThenAsync(async context =>
                {
                    await GrantLicenseEventProcessed.Task;
                })
                .Then(context =>
                {
                    context.Saga.CorrelationId = context.Message.CorrelationId;
                    context.Saga.Payload = context.Message.Payload;
                    context.Saga.Quantity = context.Message.Quantity;
                    context.Saga.Time = context.Message.Time;
                    context.Saga.WhichSpec = context.Message.WhichSpec;
                })
                .Produce(context => context.Init<LicenseNotificationEvent>(new
                {
                    context.Saga

                    //context.Saga.CorrelationId,
                    //context.Saga.Payload,
                    //context.Saga.Quantity,
                    //context.Saga.Time,
                    //context.Saga.WhichSpec
                }))
                .TransitionTo(NotificationReady));

        During(NotificationReady,
            When(Notify)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}



