using DiscordBot.Application.Interface;
using DiscordSaga.Components.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DiscordSaga.Consumers
{
    public class LicenseNotificationEventConsumer : IConsumer<LicenseNotificationEvent>
    {
        private readonly ILogger<LicenseNotificationEventConsumer> _logger;
        private readonly IDiscordBotNotificationRepository _botNotificationRepository;

        public LicenseNotificationEventConsumer(ILogger<LicenseNotificationEventConsumer> logger, IDiscordBotNotificationRepository botNotificationRepository)
        {
            _logger = logger;
            _botNotificationRepository = botNotificationRepository;
        }

        public async Task Consume(ConsumeContext<LicenseNotificationEvent> context)
        {
            try
            {
                _logger.LogInformation("Received Notify event with CorrelationId: {CorrelationId}", context.Message.CorrelationId);

                // Perform some business logic to send the notification
                await _botNotificationRepository.NotificationHandler(new LicenseNotificationEvent
                {
                    Payload = context.Message.Payload,
                    Quantity = context.Message.Quantity,
                    Time = context.Message.Time,
                    WhichSpec = context.Message.WhichSpec
                });

                _logger.LogInformation("Sent notification with Quantity: {Quantity}, Time: {Time}, WhichSpec: {WhichSpec}",
                    context.Message.Quantity, context.Message.Time, context.Message.WhichSpec);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification Error");
            }
        }
    }
}