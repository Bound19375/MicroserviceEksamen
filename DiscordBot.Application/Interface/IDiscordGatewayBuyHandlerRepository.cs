using Crosscutting.KafkaDto.Discord;
using Crosscutting.SellixPayload;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordGatewayBuyHandlerRepository
    {
        Task<LicenseNotificationEvent> OrderHandler(SellixPayloadNormal.Root root);
    }
}
