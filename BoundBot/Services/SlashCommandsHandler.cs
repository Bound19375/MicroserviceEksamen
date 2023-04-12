using Crosscutting;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace BoundBot.Services
{
    internal class SlashCommandsHandler
    {
        private DiscordSocketClient Client { get; }
        private IConfiguration Configuration { get; }
        private readonly HttpClient _httpClient;


        public SlashCommandsHandler(DiscordSocketClient client, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            Client = client;
            Configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("httpClient");
        }

        internal class RestModel
        {
            public DiscordModelDto Model { get; set; } = new DiscordModelDto();

            public RestModel(SocketSlashCommand command)
            {
                var guilduser = (SocketGuildUser)command.User;

                Model.DiscordUsername = guilduser.Username + "#" + guilduser.Discriminator;
                Model.DiscordId = guilduser.Id.ToString();
                Model.Channel = command.Channel.Id.ToString();
                Model.Command = command.CommandName;

                var roles = guilduser.Roles.Select(role => role.ToString()).ToList();
                Model.Roles = roles.Count == 0 ? new List<string>() { "0000000000" } : roles;
                Model.RefreshToken = string.Empty;
            }
        }
        private class JwtDto
        {
            public string AccessToken { get; set; }
            public DateTime ExpiresIn { get; set; } 
            public string RefreshToken { get; set; } 
            //Use File to save & read refreshtoken
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
                    foreach (var role in restModel.Model.Roles!)
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
                        foreach (var role in restModel.Model.Roles!)
                        {
                            Console.Write(role + ", ");
                        }
                    }
                }

                var embedBuiler = new EmbedBuilder();
                embedBuiler.ThumbnailUrl = "https://i.imgur.com/dxCVy9r.png";

                embedBuiler.AddField("Purchase",
                    "Please use the following information I've collected for you upon your purchase!" +
                    $"\r\n\r\nYour Discord Name Is: `{restModel.Model.DiscordUsername}`" +
                    $"\r\nYour Discord ID Is: `{restModel.Model.DiscordId}`" +
                    "\r\nYour A# Hwid Is: Check A# Console." +
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
                var client = _httpClient;

                RestModel restModel = new(command);

                HttpResponseMessage jwtResponseMessage = await client.PostAsJsonAsync($"/gateway/API/DiscordBot/JwtGenerate", restModel.Model);
                var jwtResponseBody = await jwtResponseMessage.Content.ReadAsStringAsync();
                var jwtResponseBodyDeserialization = JsonConvert.DeserializeObject<JwtDto>(jwtResponseBody) ?? new JwtDto();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtResponseBodyDeserialization.AccessToken);

                HttpResponseMessage resp = await client.PutAsJsonAsync("/gateway/API/DiscordBot/Command/UpdateDiscord", restModel.Model);
                var responseBody = await resp.Content.ReadAsStringAsync();


                var embedBuilder = new EmbedBuilder();
                embedBuilder.ThumbnailUrl = "https://i.imgur.com/dxCVy9r.png";
                if (resp.IsSuccessStatusCode)
                {

                    embedBuilder.AddField("Update Discord",
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

        private async Task Hwid(SocketSlashCommand command)
        {
            try
            {
                var client = _httpClient;

                RestModel restModel = new(command);



                restModel.Model.Hwid = (command.Data.Options.First().Value as string)!;

                HttpResponseMessage jwtResponseMessage = await client.PostAsJsonAsync($"/gateway/API/DiscordBot/JwtGenerate", restModel.Model);
                var jwtResponseBody = await jwtResponseMessage.Content.ReadAsStringAsync();
                var jwtResponseBodyDeserialization = JsonConvert.DeserializeObject<JwtDto>(jwtResponseBody) ?? new JwtDto();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtResponseBodyDeserialization.AccessToken);

                HttpResponseMessage resp = await client.PutAsJsonAsync("/gateway/API/DiscordBot/Command/UpdateHwid", restModel.Model);
                var responseBody = await resp.Content.ReadAsStringAsync();

                var embedBuilder = new EmbedBuilder();
                embedBuilder.ThumbnailUrl = "https://i.imgur.com/dxCVy9r.png";

                if (resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[POST REST] Successfully executed for {command.CommandName}");

                    embedBuilder.AddField("Hwid Reset",
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
                var client = _httpClient;

                RestModel restModel = new(command);

                HttpResponseMessage jwtResponseMessage = await client.PostAsJsonAsync($"/gateway/API/DiscordBot/JwtGenerate", restModel.Model);
                var jwtResponseBody = await jwtResponseMessage.Content.ReadAsStringAsync();
                var jwtResponseBodyDeserialization = JsonConvert.DeserializeObject<JwtDto>(jwtResponseBody) ?? new JwtDto();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtResponseBodyDeserialization.AccessToken);

                HttpResponseMessage resp = await client.GetAsync($"/gateway/API/DiscordBot/Query/CheckMe/{command.User.Username + " % 23" + command.User.Discriminator}/{command.User.Id}");

                var responseBody = await resp.Content.ReadAsStringAsync();

                var embedBuilder = new EmbedBuilder();
                embedBuilder.ThumbnailUrl = "https://i.imgur.com/dxCVy9r.png";
                if (resp.IsSuccessStatusCode)
                {
                    var authModels = JsonConvert.DeserializeObject<List<AuthModelDTO>>(responseBody);
                    StringBuilder builder = new StringBuilder();

                    foreach (var item in authModels!)
                    {
                        builder.AppendLine($"DiscordUsername: {item.DiscordUsername}");
                        builder.AppendLine($"DiscordId: {item.DiscordId}");
                        builder.AppendLine($"Name: {item.ProductName}");
                        builder.AppendLine($"Hwid: {item.HWID}");
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

                if (restModel.Model.Roles!.Contains("860603777790771211") || restModel.Model.Roles.Contains("860628656259203092")
                    || restModel.Model.Roles.Contains("Mod") || restModel.Model.Roles.Contains("Staff"))
                {
                    var client = _httpClient;

                    var options = command.Data.Options.First().Value as IUser;

                    HttpResponseMessage jwtResponseMessage = await client.PostAsJsonAsync($"/gateway/API/DiscordBot/JwtGenerate", restModel.Model);
                    var jwtResponseBody = await jwtResponseMessage.Content.ReadAsStringAsync();
                    var jwtResponseBodyDeserialization = JsonConvert.DeserializeObject<JwtDto>(jwtResponseBody) ?? new JwtDto();

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtResponseBodyDeserialization.AccessToken);

                    HttpResponseMessage resp = await client.GetAsync($"/gateway/API/DiscordBot/Query/CheckDB/{options!.Username + "%23" + options.Discriminator}/{options.Id}");
                    var responseBody = await resp.Content.ReadAsStringAsync();


                    if (resp.IsSuccessStatusCode)
                    {
                        var authModels = JsonConvert.DeserializeObject<List<AuthModelDTO>>(responseBody);

                        StringBuilder builder = new StringBuilder();

                        foreach (var item in authModels!)
                        {
                            builder.AppendLine($"DiscordUsername: {item.DiscordUsername}");
                            builder.AppendLine($"DiscordId: {item.DiscordId}");
                            builder.AppendLine($"Firstname: {item.Firstname}");
                            builder.AppendLine($"Lastname: {item.Lastname}");
                            builder.AppendLine($"Email: {item.Email}");
                            builder.AppendLine($"Name: {item.ProductName}");
                            builder.AppendLine($"Hwid: {item.HWID}");
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

                if (restModel.Model.Roles!.Contains("860603777790771211") || restModel.Model.Roles.Contains("860628656259203092")
                    || restModel.Model.Roles.Contains("Mod") || restModel.Model.Roles.Contains("Staff"))
                {
                    restModel.Model.Hwid = (command.Data.Options.First().Value as string)!;

                    var client = _httpClient;
                    HttpResponseMessage jwtResponseMessage = await client.PostAsJsonAsync($"/gateway/API/DiscordBot/JwtGenerate", restModel.Model);
                    var jwtResponseBody = await jwtResponseMessage.Content.ReadAsStringAsync();
                    var jwtResponseBodyDeserialization = JsonConvert.DeserializeObject<JwtDto>(jwtResponseBody) ?? new JwtDto();

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtResponseBodyDeserialization.AccessToken);

                    HttpResponseMessage resp = await client.PostAsJsonAsync($"/gateway/API/DiscordBot/Command/StaffLicense", restModel.Model);
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
