using Crosscutting.SellixPayload;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordGatewayBuyHandlerRepository
    {
        Task OrderHandler(SellixPayloadNormal.Root root);
    }
}
