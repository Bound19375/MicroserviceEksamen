using DiscordSaga.Components.Discord;

namespace DiscordBot.Application.Interface;

public interface IDiscordBotNotificationRepository
{
    Task NotificationHandler(LicenseNotificationEvent context);
}