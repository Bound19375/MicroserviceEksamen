using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crosscutting;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordBotCommandImplementation
    {
        Task<string> UpdateHWID(DiscordModelDTO model);
        Task<string> UpdateDiscordAndRole(DiscordModelDTO model);
        Task<string> GetStaffLicense(DiscordModelDTO model);
    }
}
