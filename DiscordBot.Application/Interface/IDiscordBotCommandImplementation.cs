using Crosscutting;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordBotCommandImplementation
    {
        Task<string> UpdateHwid(DiscordModelDto model);
        Task<string> UpdateDiscordAndRole(DiscordModelDto model);
        Task<string> GetStaffLicense(DiscordModelDto model);
    }
}
