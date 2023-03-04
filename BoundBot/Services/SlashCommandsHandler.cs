using Crosscut;
using Discord.WebSocket;
using System.Configuration;
using System.Net.Http.Json;
using System.Text;
using Crosscutting;
using Discord;
using Newtonsoft.Json;
using Discord.Rest;

namespace BoundBot.Services
{
    internal class SlashCommandsHandler
    {
        private string? InternalWebhook { get; } = ConfigurationManager.AppSettings["internalAPIDiscordBot"];

        private DiscordSocketClient Client { get; set; }

        public SlashCommandsHandler(DiscordSocketClient client)
        {
            Client = client;
        }

        internal class RestModel
        {
            public DiscordModelDTO model { get; set; } = new DiscordModelDTO();

            public RestModel(SocketSlashCommand command)
            {
                var guilduser = (SocketGuildUser)command.User;

                model.DiscordUsername = guilduser.Username + "#" + guilduser.Discriminator;
                model.DiscordId = guilduser.Id.ToString();
                model.Channel = command.Channel.Id.ToString();
                model.Command = command.CommandName;
                
                var roles = guilduser.Roles.Select(role => role.ToString()).ToList();
                model.Roles = roles.Count == 0 ? new List<string>() { "0000000000" } : roles;
            }
        }
        
        public async Task SlashCommandHandler(SocketSlashCommand command)
        {
            // Let's add a switch statement for the command name so we can handle multiple commands in one event.
            switch (command.Data.Name)
            {
                case "help":
                case null:
                    await Help(command);
                    break;

                case "purchase":
                    await Purchase(command);
                    break;

                case "updatediscord":
                    await UpdateDiscord(command);
                    break;

                case "hwid":
                    await Hwid(command);
                    break;

                case "checkme":
                    await CheckMe(command);
                    break;

                case "checkdb":
                    await CheckDb(command);
                    break;

                case "stafflicense":
                    await StaffLicense(command);
                    break;
            }
        }

        private async Task Help(SocketSlashCommand command)
        {
            string userCommands = 
                "`/purchase -- Get shop link & required information`" +
                "\n`/updatediscord -- Updates Discord Role & Db Information`" +
                "\n`/hwid + {option:hwid} -- Updates DbHwid`" +
                "\n`/checkme -- Get Current Active Licenses`" + 
                "\n" +
                "\n";
            
            string staffCommands = 
                "`/checkdb + {option:Iuser}`" +
                "\n`/stafflicense + {option:hwid}`";

            RestModel restModel = new(command);
            foreach (var ele in restModel.GetType().GetProperties())
            {
                if (ele.Name != "Roles")
                {
                    Console.WriteLine($"{ele.Name}: {ele.GetValue(restModel)}");
                }
                else
                {
                    Console.Write("Roles: ");
                    foreach (var role in restModel.model.Roles!)
                    {
                        Console.Write(role + ", ");
                    }
                }
            }

            var embed = new EmbedBuilder()
                            .WithThumbnailUrl("https://i.imgur.com/dxCVy9r.png")
                            .WithDescription($"Commands can only be executed in #{Client.GetChannelAsync(879325830021529630)}")
                            .AddField("User Commands", userCommands)
                            .AddField("Staff Commands", staffCommands)
                            .WithColor(Color.DarkOrange)
                            .WithCurrentTimestamp()
                            .Build();

            // Now, Let's respond with the embed.
            await command.RespondAsync(embed: embed);
        }

        private async Task Purchase(SocketSlashCommand command)
        {
            try
            {
                RestModel restModel = new(command);

                Console.WriteLine($"[POST REST] Successfully executed for {command.CommandName}");
                foreach (var ele in restModel.GetType().GetProperties())
                {
                    if (ele.Name != "Roles")
                    {
                        Console.WriteLine($"{ele.Name}: {ele.GetValue(restModel)}");
                    }
                    else
                    {
                        Console.Write("Roles: ");
                        foreach (var role in restModel.model.Roles)
                        {
                            Console.Write(role + ", ");
                        }
                    }
                }

                var embedBuiler = new EmbedBuilder();
                embedBuiler.ThumbnailUrl = "https://i.imgur.com/dxCVy9r.png";

                embedBuiler.AddField("Purchase",
                    "Please use the following information I've collected for you upon your purchase!" +
                    $"\r\n\r\nYour Discord Name Is: `{restModel.model.DiscordUsername}`" +
                    $"\r\nYour Discord ID Is: `{restModel.model.DiscordId}`" +
                    "\r\nYour A# HWID Is: Check A# Console." +
                    "\r\n\r\nIf you're a server booster contact @Bound for your coupon for 10%." +
                    "\r\n\r\nThank you for your interest & support in what I do! <:peepolove:1002285157132271746>");

                embedBuiler.WithColor(Color.DarkOrange);
                embedBuiler.WithCurrentTimestamp();
                await command.RespondAsync(embed: embedBuiler.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task UpdateDiscord(SocketSlashCommand command)
        {
            try
            {
                var client = new HttpClient();

                RestModel restModel = new(command);

                var embedBuilder = new EmbedBuilder();
                embedBuilder.ThumbnailUrl = "https://i.imgur.com/dxCVy9r.png";

                HttpResponseMessage resp = await client.PutAsJsonAsync(InternalWebhook + "/gateway/API/DiscordBot/Command/UpdateDiscord", restModel.model);
                var ResponseBody = await resp.Content.ReadAsStringAsync();
                if (resp.IsSuccessStatusCode)
                {
                    
                    embedBuilder.AddField("Update Discord",
                        $"\n{ResponseBody}");
                }
                else
                {
                    embedBuilder.AddField("BadRequest", ResponseBody);
                }

                embedBuilder.WithColor(Color.DarkOrange);
                embedBuilder.WithCurrentTimestamp();
                await command.RespondAsync(embed: embedBuilder.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task Hwid(SocketSlashCommand command)
        {
            try
            {
                var client = new HttpClient();

                RestModel restModel = new(command);

                var embedBuilder = new EmbedBuilder();
                embedBuilder.ThumbnailUrl = "https://i.imgur.com/dxCVy9r.png";

                restModel.model.HWID = command.Data.Options.First().Value as string;

                HttpResponseMessage resp = await client.PutAsJsonAsync(InternalWebhook + "/gateway/API/DiscordBot/Command/UpdateHwid", restModel.model);
                var responseBody = await resp.Content.ReadAsStringAsync();
                if (resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[POST REST] Successfully executed for {command.CommandName}");

                    embedBuilder.AddField("HWID Reset",
                        $"\n{responseBody}");
                }
                else
                {
                    embedBuilder.AddField("BadRequest", responseBody);
                }

                embedBuilder.WithColor(Color.DarkOrange);
                embedBuilder.WithCurrentTimestamp();
                await command.RespondAsync(embed: embedBuilder.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task CheckMe(SocketSlashCommand command)
        {
            try
            {
                var client = new HttpClient();

                RestModel restModel = new(command);

                var embedBuilder = new EmbedBuilder();
                embedBuilder.ThumbnailUrl = "https://i.imgur.com/dxCVy9r.png";

                HttpResponseMessage resp = await client.GetAsync(InternalWebhook + $"/gateway/API/DiscordBot/Query/CheckMe/{command.User.Username+ " % 23" + command.User.Discriminator}/{command.User.Id}");

                var responseBody = await resp.Content.ReadAsStringAsync();
                if (resp.IsSuccessStatusCode)
                {
                    var authModels = JsonConvert.DeserializeObject<List<AuthModelDTO>>(responseBody);
                    StringBuilder builder = new StringBuilder();

                    foreach (var item in authModels)
                    {
                        builder.AppendLine($"DiscordUsername: {item.DiscordUsername}");
                        builder.AppendLine($"DiscordId: {item.DiscordId}");
                        builder.AppendLine($"Name: {item.ProductName}");
                        builder.AppendLine($"HWID: {item.HWID}");
                        builder.AppendLine($"EndDate: {item.EndDate}");
                        builder.AppendLine("\n");
                    }

                    embedBuilder.AddField("CheckDB",
                        $"\n{builder}");

                    embedBuilder.WithColor(Color.DarkOrange);
                }
                else
                {
                    embedBuilder.AddField("BadRequest", responseBody);
                }

                embedBuilder.WithCurrentTimestamp();
                await command.RespondAsync(embed: embedBuilder.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        //Admin
        private async Task CheckDb(SocketSlashCommand command)
        {
            try
            {
                RestModel restModel = new(command);
                var embedBuilder = new EmbedBuilder();
                embedBuilder.ThumbnailUrl = "https://i.imgur.com/dxCVy9r.png";


                if (restModel.model.Roles.Contains("860603777790771211") || restModel.model.Roles.Contains("860628656259203092")
                    || restModel.model.Roles.Contains("Mod") || restModel.model.Roles.Contains("Staff"))
                {
                    var client = new HttpClient();

                    var options = command.Data.Options.First().Value as IUser;

                    HttpResponseMessage resp = await client.GetAsync(InternalWebhook + $"/gateway/API/DiscordBot/Query/CheckDB/{options.Username+ "%23" + options.Discriminator}/{options.Id}");
                    var responseBody = await resp.Content.ReadAsStringAsync();
                    if (resp.IsSuccessStatusCode)
                    {
                        var authModels = JsonConvert.DeserializeObject<List<AuthModelDTO>>(responseBody);

                        StringBuilder builder = new StringBuilder();

                        foreach (var item in authModels)
                        {
                            builder.AppendLine($"DiscordUsername: {item.DiscordUsername}");
                            builder.AppendLine($"DiscordId: {item.DiscordId}");
                            builder.AppendLine($"Firstname: {item.Firstname}");
                            builder.AppendLine($"Lastname: {item.Lastname}");
                            builder.AppendLine($"Email: {item.Email}");
                            builder.AppendLine($"Name: {item.ProductName}");
                            builder.AppendLine($"HWID: {item.HWID}");
                            builder.AppendLine($"EndDate: {item.EndDate}");
                            builder.AppendLine("\n");
                        }

                        embedBuilder.AddField("CheckDB",
                            $"\n{builder}");

                        embedBuilder.WithColor(Color.DarkOrange);
                    }
                    else
                    {
                        embedBuilder.AddField("BadRequest", responseBody);
                    }
                }
                else
                {
                    embedBuilder.AddField("Denied", "You do not have access to staff commands!");
                }

                embedBuilder.WithCurrentTimestamp();
                await command.RespondAsync(embed: embedBuilder.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task StaffLicense(SocketSlashCommand command)
        {
            try
            {
                RestModel restModel = new(command);
                var embedBuilder = new EmbedBuilder();
                embedBuilder.ThumbnailUrl = "https://i.imgur.com/dxCVy9r.png";

                if (restModel.model.Roles.Contains("860603777790771211") || restModel.model.Roles.Contains("860628656259203092")
                    || restModel.model.Roles.Contains("Mod") || restModel.model.Roles.Contains("Staff"))
                {
                    restModel.model.HWID = command.Data.Options.First().Value as string;

                    var client = new HttpClient();

                    HttpResponseMessage resp = await client.PostAsJsonAsync(InternalWebhook + $"/gateway/API/DiscordBot/Command/StaffLicense", restModel.model);
                    var responseBody = await resp.Content.ReadAsStringAsync();
                    if (resp.IsSuccessStatusCode)
                    {
                        embedBuilder.AddField("Stafflicense",
                            $"\n\n{responseBody}");

                        embedBuilder.WithColor(Color.DarkOrange);
                    }
                    else
                    {
                        embedBuilder.AddField("BadRequest", responseBody);
                    }
                }
                else
                {
                    embedBuilder.AddField("Denied", "You do not have access to staff commands!");
                }

                embedBuilder.WithCurrentTimestamp();
                await command.RespondAsync(embed: embedBuilder.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
