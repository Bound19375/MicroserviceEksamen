using Crosscutting.SellixPayload;
using DiscordBot.Application.Interface;
using DiscordSaga.Components.Discord;
using MassTransit;

namespace DiscordBot.Application.Implementation
{
    public class DiscordGatewayBuyHandlerImplementation : IDiscordGatewayBuyHandlerImplementation
    {
        private readonly ITopicProducer<LicenseGrantEvent> _topicProducer;
        private readonly IPublishEndpoint _publishEndpoint;


        public DiscordGatewayBuyHandlerImplementation(ITopicProducer<LicenseGrantEvent> topicProducer, IPublishEndpoint publishEndpoint)
        {
            _topicProducer = topicProducer;
            _publishEndpoint = publishEndpoint;
        }

        async Task IDiscordGatewayBuyHandlerImplementation.GrantLicense(SellixPayloadNormal.Root root)
        {
            try
            {
                await _topicProducer.Produce(new LicenseGrantEvent
                {
                    CorrelationId = Guid.NewGuid(),
                    Payload = root
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
