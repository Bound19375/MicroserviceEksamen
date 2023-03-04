using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoundBot.Services
{
    internal class SlashCommandsBuilder
    {
        private DiscordSocketClient Client { get; set; }


        public SlashCommandsBuilder(DiscordSocketClient client)
        {
            Client = client;
        }

        public async Task Client_Ready()
        {
            // Let's build a guild command! We're going to need a guild so lets just put that in a variable.
            var guild = Client.GetGuild(860603152280584222);

            // Next, lets create our slash command builder. This is like the embed builder but for slash commands.

            List<ApplicationCommandProperties> commands = new();
            // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
            // Descriptions can have a max length of 100.
            var guildCommand = new SlashCommandBuilder();
            guildCommand.WithName("help");
            guildCommand.WithDescription("Get a list of commands");
            commands.Add(guildCommand.Build());

            guildCommand = new();
            guildCommand.WithName("purchase");
            guildCommand.WithDescription("Get shop link & required information");
            commands.Add(guildCommand.Build());

            guildCommand = new();
            guildCommand.WithName("updatediscord");
            guildCommand.WithDescription("Update your discord name & id for active license(s) & get according role");
            commands.Add(guildCommand.Build());

            guildCommand = new();
            guildCommand.WithName("hwid");
            guildCommand.WithDescription("Update your hwid for active license(s)");
            guildCommand.AddOption("hwid", ApplicationCommandOptionType.String, "hwid from your A# console", true);
            commands.Add(guildCommand.Build());

            guildCommand = new();
            guildCommand.WithName("checkme");
            guildCommand.WithDescription("Check your active license(s)");
            commands.Add(guildCommand.Build());

            //Staff
            guildCommand =new();
            guildCommand.WithName("checkdb");
            guildCommand.WithDescription("Check active license(s) for user");
            guildCommand.AddOption("user", ApplicationCommandOptionType.User, "User", true);
            commands.Add(guildCommand.Build());

            guildCommand = new();
            guildCommand.WithName("stafflicense");
            guildCommand.WithDescription("Grant yourself a staff license");
            guildCommand.AddOption("hwid", ApplicationCommandOptionType.String, "hwid from your A# console", true);
            commands.Add(guildCommand.Build());

            try
            {
                await Client.BulkOverwriteGlobalApplicationCommandsAsync(commands.ToArray());
               
                /*
                foreach (var ele in commands)
                {
                    await guild.CreateApplicationCommandAsync(ele);
                }
                */

                // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.

                // With global commands we don't need the guild.
                // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
                // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
            }
            catch (HttpException exception)
            {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                Console.WriteLine(json);
            }
        }
    }
}
