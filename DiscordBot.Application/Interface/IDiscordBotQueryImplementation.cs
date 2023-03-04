using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crosscutting;

namespace DiscordBot.Application.Interface
{
    public interface IDiscordBotQueryImplementation
    {
        Task<List<AuthModelDTO>> CheckDB(string username, string id);
        Task<List<AuthModelDTO>> CheckMe(string username, string id);
    }
}
