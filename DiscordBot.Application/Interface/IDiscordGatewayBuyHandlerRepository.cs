using Crosscutting.KafkaDto.Discord;
using Crosscutting.SellixPayload;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordGatewayBuyHandlerRepository
    {
        Task<KafkaDiscordSagaMessageDto> OrderHandler(SellixPayloadNormal.Root root);
    }
}
