using Crosscutting.SellixPayload;
using DiscordNetConsumers;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordGatewayBuyHandlerRepository
    {
        Task<KafkaDiscordSagaMessageDto> OrderHandler(SellixPayloadNormal.Root root);
    }
}
