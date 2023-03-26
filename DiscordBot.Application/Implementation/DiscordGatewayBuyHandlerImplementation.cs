using Crosscutting.KafkaDto.Discord;
using Crosscutting.SellixPayload;
using DiscordBot.Application.Interface;
using MassTransit;

namespace DiscordBot.Application.Implementation
{
    public class DiscordGatewayBuyHandlerImplementation : IDiscordGatewayBuyHandlerImplementation
    {
        private readonly ITopicProducer<LicenseNotificationEvent> _topicProducer;


        public DiscordGatewayBuyHandlerImplementation(ITopicProducer<LicenseNotificationEvent> topicProducer)
        {
            _topicProducer = topicProducer;
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
