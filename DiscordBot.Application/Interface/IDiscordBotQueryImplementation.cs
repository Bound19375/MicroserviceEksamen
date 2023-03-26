using Crosscutting;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordBotQueryImplementation
    {
        Task<List<AuthModelDTO>> CheckDB(string username, string id);
        Task<List<AuthModelDTO>> CheckMe(string username, string id);
    }
}
