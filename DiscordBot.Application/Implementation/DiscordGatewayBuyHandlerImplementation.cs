using Crosscutting.KafkaDto.Discord;
using Crosscutting.SellixPayload;
using DiscordBot.Application.Interface;
using MassTransit;

namespace DiscordBot.Application.Implementation
{
    public class DiscordGatewayBuyHandlerImplementation : IDiscordGatewayBuyHandlerImplementation
    {
        private readonly ITopicProducer<KafkaDiscordSagaMessageDto> _topicProducer;


        public DiscordGatewayBuyHandlerImplementation(ITopicProducer<KafkaDiscordSagaMessageDto> topicProducer)
        {
            _topicProducer = topicProducer;
        }

        async Task IDiscordGatewayBuyHandlerImplementation.GrantLicense(SellixPayloadNormal.Root root)
        {
            try
            {
                await _topicProducer.Produce(new KafkaDiscordSagaMessageDto
                {
                    Payload = root,
                    State = DiscordTransportMessageState.LicenseGrantReady
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
