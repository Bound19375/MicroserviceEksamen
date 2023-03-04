using Crosscutting;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordBotCommandRepository
    {
        Task<string> UpdateHWID(DiscordModelDTO model);
        Task<string> UpdateDiscordAndRole(DiscordModelDTO model);
        Task<string> GetStaffLicense(DiscordModelDTO model);
    }
}
