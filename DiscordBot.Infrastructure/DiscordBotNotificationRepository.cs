using Crosscutting;
using Crosscutting.DiscordConnectionHandler.DiscordClientLibrary;
using Crosscutting.KafkaDto.Discord;
using Discord;
using Discord.WebSocket;
using DiscordBot.Application.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Infrastructure;

public class DiscordBotNotificationRepository : IDiscordBotNotificationRepository
{
    private readonly DiscordSocketClient _client = DiscordClient.GetDiscordSocketClient();
    private readonly ILogger<DiscordBotNotificationRepository> _logger;
    private readonly IConfiguration _configuration;

    public DiscordBotNotificationRepository(ILogger<DiscordBotNotificationRepository> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    async Task IDiscordBotNotificationRepository.NotificationHandler(LicenseNotificationEvent context)
    {
        var clientUser = await _client.GetUserAsync(ulong.Parse(context.Payload!.Data.CustomFields.DiscordId));

        if (clientUser != null)
        {
            var guild = _client.GetGuild(ulong.Parse(_configuration["Discord:Guid"]!));
            IGuildUser? guildUser = null;
            if (guild != null)
            {
                guildUser = guild.GetUser(clientUser.Id);
            }

            if (guildUser != null && guild != null)
            {
                ulong roleId = (ulong)(context.WhichSpec == WhichSpec.AIO ? 986361482377826334 : 911959454323445840);
                var role = guild.GetRole(roleId);
                await guildUser.AddRoleAsync(role);
            }

            bool couldSendToUser = false;
            try
            {
                var embed = new EmbedBuilder()
                    .WithThumbnailUrl("https://i.imgur.com/dxCVy9r.png")
                    .AddField("Confirmation", $"You've been successfully added to the database & roled!" +
                                              $"\nOrderId: {context.Payload.Data.Uniqid}" +
                                              $"\nProduct: {context.Payload.Data.ProductTitle}" +
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
                _logger.LogInformation("Wasn't able to DM: " + context.Payload.Data.CustomFields.DiscordUser);
            }

            var privateEmbed = new EmbedBuilder()
                .WithThumbnailUrl("https://i.imgur.com/dxCVy9r.png")
                .AddField("Confirmation",
                    $"\n{clientUser.Mention} has been successfully added to the database & roled!" +
                    $"\nUser Notified: {couldSendToUser}" +
                    $"\nOrderId: {context.Payload.Data.Uniqid}" +
                    $"\nProduct: {context.Payload.Data.ProductTitle}" +
                    $"\nEndDate: {context.Time.AddDays(Convert.ToInt32(1 * context.Quantity))}")
                .WithColor(Color.DarkOrange)
                .WithCurrentTimestamp()
                .Build();

            var privateChannel = await _client.GetChannelAsync(862658521065848872); //NotifyChannel
            var textNotifier = privateChannel as SocketTextChannel;
            await textNotifier!.SendMessageAsync("", false, privateEmbed);
        }
    }
}