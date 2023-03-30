using DiscordSaga.Components.KafkaDto.Discord;

namespace DiscordBot.Application.Interface;

public interface IDiscordBotNotificationRepository
{
    Task NotificationHandler(LicenseNotificationEvent context);
}