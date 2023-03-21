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
        Task<string> UpdateHWID(DiscordModelDto model);
        Task<string> UpdateDiscordAndRole(DiscordModelDto model);
        Task<string> GetStaffLicense(DiscordModelDto model);
    }
}
