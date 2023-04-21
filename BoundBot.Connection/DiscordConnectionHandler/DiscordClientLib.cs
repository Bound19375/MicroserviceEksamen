using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BoundBot.Connection.DiscordConnectionHandler
{
    namespace DiscordClientLibrary
    {
        public static class DiscordClient
        {
            #pragma warning disable CS8618
            private static readonly DiscordSocketClient Client = new(new DiscordSocketConfig
            {
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.All
            });
            private static CommandService _command;
            #pragma warning restore CS8618

            public static DiscordSocketClient GetDiscordSocketClient(string token)
            {
                if (Client.ConnectionState != ConnectionState.Connected)
                {

                    Client.SetGameAsync("/Help", null, ActivityType.Playing).Wait();
                    Client.LoginAsync(TokenType.Bot, token).Wait();
                    Client.StartAsync().Wait();

                    while (Client.ConnectionState != ConnectionState.Connected)
                    {
                        Task.Delay(100).Wait();
                    }
                }

                return Client;
            }

            public static CommandService GetCommandService()
            {
                _command ??= new CommandService();

                return _command;
            }
        }
    }
}
