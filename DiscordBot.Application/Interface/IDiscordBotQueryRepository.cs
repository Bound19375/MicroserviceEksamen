using Crosscutting;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordBotQueryRepository
    {
        Task<List<AuthModelDTO>> CheckDB(string username, string id);
        Task<List<AuthModelDTO>> CheckMe(string username, string id);

    }
}