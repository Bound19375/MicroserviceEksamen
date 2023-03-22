using Crosscutting.SellixPayload;
using DiscordNetConsumers;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordGatewayBuyHandlerRepository
    {
        Task<KafkaNotificationMessageDto> OrderHandler(SellixPayloadNormal.Root root);
    }
}
