using System.Text.Json.Nodes;
using Crosscutting.SellixPayload;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordGatewayBuyHandlerImplementation
    {
        Task<bool> GrantLicense(JsonObject root);
    }
}
