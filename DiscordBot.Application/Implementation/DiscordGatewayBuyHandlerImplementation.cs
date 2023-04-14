using System.Text.Json.Nodes;
using Crosscutting.SellixPayload;
using DiscordBot.Application.Interface;
using DiscordSaga.Components.Events;
using MassTransit;
using Newtonsoft.Json;

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

        async Task<bool> IDiscordGatewayBuyHandlerImplementation.GrantLicense(JsonObject root)
        {
            try
            {
                await _topicProducer.Produce(new LicenseGrantEvent
                {
                    CorrelationId = Guid.NewGuid(),
                    Payload = root.ToJsonString()
                });

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
