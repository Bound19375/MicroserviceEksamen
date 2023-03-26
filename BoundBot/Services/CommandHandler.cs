using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

namespace BoundBot.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(DiscordSocketClient client, CommandService commands)
        {
            _commands = commands;
            _client = client;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;
            _commands.CommandExecuted += OnCommandExecutedAsync;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null);
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // We have access to the information of the command executed,
            // the context of the command, and the result returned from the
            // execution in this event.

            // We can tell the user what went wrong
            if (!string.IsNullOrEmpty(result?.ErrorReason))
            {
                string UserCommands =
                "`/purchase -- Get shop link & required information`" +
                "\n`/updatediscord -- Updates Discord Role & Db Information`" +
                "\n`/hwid + {option:hwid} -- Updates DbHwid`" +
                "\n`/checkme -- Get Current Active Licenses`" +
                "\n" +
                "\n";

                string StaffCommands =
                    "`/checkdb + {option:Iuser}`" +
                    "\n`/stafflicense + {option:hwid}`";

                var embedBuiler = new EmbedBuilder();
                embedBuiler.ThumbnailUrl = "https://i.imgur.com/dxCVy9r.png";
                embedBuiler.WithDescription($"`{context.Message.Content}` is not a slash (/) command!");
                embedBuiler.AddField("User Commands", UserCommands);
                embedBuiler.AddField("Staff Commands", StaffCommands);
                embedBuiler.WithColor(Color.Green);
                embedBuiler.WithCurrentTimestamp();

                // Now, Let's respond with the embed.
                await context.Message.ReplyAsync(embed: embedBuiler.Build());
            }

            // ...or even log the result (the method used should fit into
            // your existing log handler)
            var commandName = command.IsSpecified ? command.Value.Name : "A command";
            Console.WriteLine((new LogMessage(LogSeverity.Info,
                "CommandExecution",
                $"{commandName} was executed at {DateTime.UtcNow}.")));
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            Console.WriteLine($"content: {message.Content} Bot: {message.Author.IsBot} Channel: {message.Channel.Id}");

            if (message.Channel.Id.ToString() == 879325830021529630.ToString()
                || message.Channel.Id.ToString() == 913222623150878751.ToString()
                || message.Channel.Id.ToString() == 1079815280190033950.ToString())
            {
                if (message.Author.IsBot != true
                && (message.Content != null || message.Content != String.Empty))
                {
                    // Create a WebSocket-based command context based on the message
                    var context = new SocketCommandContext(_client, message);

                    // Execute the command with the command context we just
                    // created, along with the service provider for precondition checks.
                    await _commands.ExecuteAsync(
                        context: context,
                        argPos: argPos,
                        services: null);

                }
            }
            else
            {
                return;
            }
        }
    }
}

