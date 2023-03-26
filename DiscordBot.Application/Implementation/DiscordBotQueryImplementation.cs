using Crosscutting;
using DiscordBot.Application.Interface;

namespace DiscordBot.Application.Implementation
{
    public class DiscordBotQueryImplementation : IDiscordBotQueryImplementation
    {
        //private readonly IUnitOfWork<AuthDbContext> _UoW; //Not needed for queries
        private readonly IDiscordBotQueryRepository _query;
        public DiscordBotQueryImplementation(IDiscordBotQueryRepository query)
        {
            _query = query;
        }

        async Task<List<AuthModelDTO>> IDiscordBotQueryImplementation.CheckDB(string username, string id)
        {
            try
            {
                return await _query.CheckDB(username, id);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public async Task<List<AuthModelDTO>> CheckMe(string username, string id)
        {
            try
            {
                return await _query.CheckMe(username, id);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }
    }
}
