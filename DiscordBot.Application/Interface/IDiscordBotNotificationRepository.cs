using Crosscutting.KafkaDto.Discord;

namespace DiscordBot.Application.Interface;

public interface IDiscordBotNotificationRepository
{
    Task NotificationHandler(KafkaDiscordSagaMessageDto context);
}