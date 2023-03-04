using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Crosscutting.DiscordConnectionHandler
{
    namespace DiscordClientLibrary
    {
        public static class DiscordClient
        {
            private static DiscordSocketClient _client;
            private static CommandService _command;

            public static DiscordSocketClient GetDiscordSocketClient()
            {
                if (_client == null)
                {
                    _client = new DiscordSocketClient(new DiscordSocketConfig
                    {
                        AlwaysDownloadUsers = true,
                        GatewayIntents = GatewayIntents.All
                    });
                    _client.SetGameAsync("/Help", null, ActivityType.Playing).Wait();
                    _client.LoginAsync(TokenType.Bot, "MTAxNDIxMzg0NzQ3MDU3NTY0Ng.GTchGK.2o1Nbp0JMMHrKOoyigdm9UjZhSqtv5IrTewARE").Wait();
                    _client.StartAsync().Wait();
                }
                return _client;
            }

            public static CommandService GetCommandService()
            {
                _command ??= new CommandService();

                return _command;
            }
        }
    }
}
