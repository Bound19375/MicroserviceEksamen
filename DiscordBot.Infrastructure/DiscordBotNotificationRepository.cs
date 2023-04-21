using BoundBot.Connection.DiscordConnectionHandler.DiscordClientLibrary;
using Crosscutting;
using Crosscutting.SellixPayload;
using Discord;
using Discord.WebSocket;
using DiscordBot.Application.Interface;
using DiscordSaga.Components.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DiscordBot.Infrastructure;

public class DiscordBotNotificationRepository : IDiscordBotNotificationRepository
{
    private readonly ILogger<DiscordBotNotificationRepository> _logger;
    private readonly IConfiguration _configuration;

    public DiscordBotNotificationRepository(ILogger<DiscordBotNotificationRepository> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    async Task IDiscordBotNotificationRepository.NotificationHandler(LicenseNotificationEvent context)
    {
        try
        {
            DiscordSocketClient client =
                DiscordClient.GetDiscordSocketClient(_configuration["Discord:Token"] ?? string.Empty);

            var deserializePayload =
                JsonConvert.DeserializeObject<SellixPayloadNormal.Root>(context.Payload ??
                                                                        throw new Exception(
                                                                            "Notification Deserialization Failure"));

            if (deserializePayload != null)
            {
                var clientUser = await client.GetUserAsync(ulong.Parse(deserializePayload.Data.CustomFields.DiscordId));

                if (clientUser != null)
                {
                    var guild = client.GetGuild(ulong.Parse(_configuration["Discord:Guid"]!));
                    IGuildUser? guildUser = null;
                    if (guild != null)
                    {
                        guildUser = guild.GetUser(clientUser.Id);
                    }

                    if (guildUser != null && guild != null)
                    {
                        ulong roleId = (ulong)(context.WhichSpec == WhichSpec.AIO
                            ? 986361482377826334
                            : 911959454323445840);
                        var role = guild.GetRole(roleId);
                        await guildUser.AddRoleAsync(role);
                    }

                    bool couldSendToUser = false;
                    try
                    {
                        var embed = new EmbedBuilder()
                            .WithThumbnailUrl("https://i.imgur.com/dxCVy9r.png")
                            .AddField("Confirmation", $"You've been successfully added to the database & roled!" +
                                                      $"\nOrderId: {deserializePayload.Data.Uniqid}" +
                                                      $"\nProduct: {deserializePayload.Data.ProductTitle}" +
                                                      $"\nEndDate: {context.Time.AddDays(Convert.ToInt32(1 * context.Quantity))}" +
                                                      "\nPlease read the instruction channels & faq!")
                            .WithColor(Color.DarkOrange)
                            .WithCurrentTimestamp()
                            .Build();

                        await clientUser.SendMessageAsync("", false, embed);
                        couldSendToUser = true;
                    }
                    catch
                    {
                        _logger.LogInformation("Wasn't able to DM: " +
                                               deserializePayload.Data.CustomFields.DiscordUser);
                    }

                    var privateEmbed = new EmbedBuilder()
                        .WithThumbnailUrl("https://i.imgur.com/dxCVy9r.png")
                        .AddField("Confirmation",
                            $"\n{clientUser.Mention} has been successfully added to the database & roled!" +
                            $"\nUser Notified: {couldSendToUser}" +
                            $"\nOrderId: {deserializePayload.Data.Uniqid}" +
                            $"\nProduct: {deserializePayload.Data.ProductTitle}" +
                            $"\nEndDate: {context.Time.AddDays(Convert.ToInt32(1 * context.Quantity))}")
                        .WithColor(Color.DarkOrange)
                        .WithCurrentTimestamp()
                        .Build();

                    var privateChannel = await client.GetChannelAsync(862658521065848872); //NotifyChannel
                    var textNotifier = privateChannel as IMessageChannel;
                    await textNotifier!.SendMessageAsync("", false, privateEmbed);
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogInformation(ex.Message);
        }
    }
}