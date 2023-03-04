using Auth.Database;
using Crosscutting.SellixPayload;
using Crosscutting.TransactionHandling;
using DiscordBot.Application.Interface;


namespace DiscordBot.Application.Implementation
{
    public class DiscordGatewayBuyHandlerImplementation : IDiscordGatewayBuyHandlerImplementation
    {
        private readonly IUnitOfWork<AuthDbContext> _UoW;
        private readonly IDiscordGatewayBuyHandlerRepository _handler;

        public DiscordGatewayBuyHandlerImplementation(IUnitOfWork<AuthDbContext> uoW, IDiscordGatewayBuyHandlerRepository handler)
        {
            _UoW = uoW;
            _handler = handler;
        }

        async Task IDiscordGatewayBuyHandlerImplementation.WebShopGrantWallet(SellixPayloadNormal.Root root)
        {
            try
            {
                await _UoW.CreateTransaction(System.Data.IsolationLevel.Serializable);
                await _handler.OrderHandler(root);
                await _UoW.Commit();
            }
            catch (Exception ex)
            {
                await _UoW.Rollback();
                throw new Exception(ex.Message);
            }
        }
    }
}
