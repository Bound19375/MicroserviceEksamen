using Crosscutting.SellixPayload;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordGatewayBuyHandlerImplementation
    {
        Task GrantLicense(SellixPayloadNormal.Root root);
    }
}
