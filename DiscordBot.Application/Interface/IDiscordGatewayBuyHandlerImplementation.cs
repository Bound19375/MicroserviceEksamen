using Crosscutting.SellixPayload;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordGatewayBuyHandlerImplementation
    {
        Task WebShopGrantWallet(SellixPayloadNormal.Root root);
    }
}
