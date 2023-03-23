using Crosscutting;
using Crosscutting.DiscordConnectionHandler.DiscordClientLibrary;
using Crosscutting.SellixPayload;
using Discord;
using Discord.WebSocket;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DiscordNetConsumers 
{
    public record KafkaNotificationMessageDto {
        public SellixPayloadNormal.Root? Payload { get; init; }
        public int Quantity { get; init; }
        public DateTime Time { get; init; }
        public WhichSpec WhichSpec { get; init; }
        public DiscordTransportMessageState State { get; init; }
    }

    public enum DiscordTransportMessageState
    {
        Failed,
        Processing,
        NotificationReady
    }

    public class DiscordNotificationConsumer : IConsumer<KafkaNotificationMessageDto> 
    {
        private readonly DiscordSocketClient _client = DiscordClient.GetDiscordSocketClient();
        private readonly ILogger<DiscordNotificationConsumer> _logger;
        private readonly IConfiguration _configuration;

        public DiscordNotificationConsumer(IConfiguration configuration, ILogger<DiscordNotificationConsumer> logger) 
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<KafkaNotificationMessageDto> context) 
        {
            if (context.Message.State == DiscordTransportMessageState.NotificationReady)
            {
                var clientUser = await _client.GetUserAsync(ulong.Parse(context.Message.Payload!.Data.CustomFields.DiscordId));

                if (clientUser != null) {
                    var guild = _client.GetGuild(ulong.Parse(_configuration["Discord:Guid"]!));
                    IGuildUser? guildUser = null;
                    if (guild != null) {
                        guildUser = guild.GetUser(clientUser.Id);
                    }

                    if (guildUser != null && guild != null) {
                        ulong roleId = (ulong)(context.Message.WhichSpec == WhichSpec.AIO ? 986361482377826334 : 911959454323445840);
                        var role = guild.GetRole(roleId);
                        await guildUser.AddRoleAsync(role);
                    }

                    bool couldSendToUser = false;
                    try {
                        var embed = new EmbedBuilder()
                        .WithThumbnailUrl("https://i.imgur.com/dxCVy9r.png")
                        .AddField("Confirmation", $"You've been successfully added to the database & roled!" +
                            $"\nOrderId: {context.Message.Payload.Data.Uniqid}" +
                            $"\nProduct: {context.Message.Payload.Data.ProductTitle}" +
                            $"\nEndDate: {context.Message.Time.AddDays(Convert.ToInt32(1 * context.Message.Quantity))}" +
                            "\nPlease read the instruction channels & faq!")
                        .WithColor(Color.DarkOrange)
                        .WithCurrentTimestamp()
                        .Build();

                        await clientUser.SendMessageAsync("", false, embed);
                        couldSendToUser = true;
                    }
                    catch {
                        _logger.LogInformation("Wasn't able to DM: " + context.Message.Payload.Data.CustomFields.DiscordUser);
                    }

                    var privateEmbed = new EmbedBuilder()
                        .WithThumbnailUrl("https://i.imgur.com/dxCVy9r.png")
                        .AddField("Confirmation",
                            $"\n{clientUser.Mention} has been successfully added to the database & roled!" +
                            $"\nUser Notified: {couldSendToUser}" +
                            $"\nOrderId: {context.Message.Payload.Data.Uniqid}" +
                            $"\nProduct: {context.Message.Payload.Data.ProductTitle}" +
                            $"\nEndDate: {context.Message.Time.AddDays(Convert.ToInt32(1 * context.Message.Quantity))}")
                        .WithColor(Color.DarkOrange)
                        .WithCurrentTimestamp()
                        .Build();

                    var privateChannel = await _client.GetChannelAsync(862658521065848872); //NotifyChannel
                    var textNotifier = privateChannel as SocketTextChannel;
                    await textNotifier!.SendMessageAsync("", false, privateEmbed);
                }
            }
        }
    }
}