using Crosscutting.SellixPayload;
using DiscordSaga.Components.Events;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordGatewayBuyHandlerRepository
    {
        Task<LicenseNotificationEvent> OrderHandler(SellixPayloadNormal.Root root);
    }
}
