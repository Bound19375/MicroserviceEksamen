using Auth.Database;
using Crosscutting.SellixPayload;
using Crosscutting.TransactionHandling;
using DiscordBot.Application.Interface;
using DiscordNetConsumers;
using MassTransit;


namespace DiscordBot.Application.Implementation
{
    public class DiscordGatewayBuyHandlerImplementation : IDiscordGatewayBuyHandlerImplementation
    {
        private readonly IUnitOfWork<AuthDbContext> _uoW;
        private readonly IDiscordGatewayBuyHandlerRepository _handler;
        private readonly ITopicProducer<KafkaNotificationMessageDto> _topicProducer;


        public DiscordGatewayBuyHandlerImplementation(IUnitOfWork<AuthDbContext> uoW, IDiscordGatewayBuyHandlerRepository handler, ITopicProducer<KafkaNotificationMessageDto> topicProducer)
        {
            _uoW = uoW;
            _handler = handler;
            _topicProducer = topicProducer;
        }

        async Task IDiscordGatewayBuyHandlerImplementation.GrantLicense(SellixPayloadNormal.Root root)
        {
            try
            {
                await _uoW.CreateTransaction(System.Data.IsolationLevel.Serializable);
                var message = await _handler.OrderHandler(root);
                await _uoW.Commit();

                if (message.State == DiscordTransportMessageState.NotificationReady)
                {
                    await _topicProducer.Produce(message);
                }

            }
            catch (Exception ex)
            {
                await _uoW.Rollback();
                throw new Exception(ex.Message);
            }
        }
    }
}
