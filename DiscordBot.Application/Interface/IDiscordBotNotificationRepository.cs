using DiscordSaga.Components.Events;

namespace DiscordBot.Application.Interface;

public interface IDiscordBotNotificationRepository
{
    Task NotificationHandler(LicenseNotificationEvent context);
}