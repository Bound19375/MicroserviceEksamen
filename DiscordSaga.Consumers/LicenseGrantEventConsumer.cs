using System.Data;
using Auth.Database;
using Crosscutting.SellixPayload;
using Crosscutting.TransactionHandling;
using DiscordBot.Application.Interface;
using DiscordSaga.Components.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DiscordSaga.Consumers;

public class LicenseGrantEventConsumer : IConsumer<LicenseGrantEvent>
{
    private readonly ILogger<LicenseGrantEventConsumer> _logger;
    private readonly IUnitOfWork<AuthDbContext> _unitOfWork;
    private readonly IDiscordGatewayBuyHandlerRepository _license;
    //private readonly ITopicProducer<LicenseNotificationEvent> _producer;
    private readonly LicenseStateMachine _stateMachine;

    public LicenseGrantEventConsumer(ILogger<LicenseGrantEventConsumer> logger, IUnitOfWork<AuthDbContext> unitOfWork, IDiscordGatewayBuyHandlerRepository license, LicenseStateMachine stateMachine)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _license = license;
        _stateMachine = stateMachine;
    }
    public async Task Consume(ConsumeContext<LicenseGrantEvent> context)
    {
        try
        {
            _logger.LogInformation("Received GrantLicense Event: {id} with payload: {Payload}", context.Message.CorrelationId, context.Message.Payload);

            await _unitOfWork.CreateTransaction(IsolationLevel.Serializable);
            if (context.Message.Payload != null)
            {
                var deserializePayload =
                    JsonConvert.DeserializeObject<SellixPayloadNormal.Root>(context.Message.Payload);

                var license = await _license.OrderHandler(deserializePayload ?? throw new NullReferenceException("LicenseGrant Payload Null"));

                await _unitOfWork.Commit();

                context.Message.Payload = license.Payload;
                context.Message.Quantity = license.Quantity;
                context.Message.Time = license.Time;
                context.Message.WhichSpec = license.WhichSpec;
                context.Message.CorrelationId = context.Message.CorrelationId;

                _logger.LogInformation("Finished GrantLicense Event with CorrelationId: {CorrelationId}", context.Message.CorrelationId);

                _stateMachine.GrantLicenseEventProcessed.SetResult(true);

            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "License Consumer Error");
        }
    }
}