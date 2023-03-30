using Crosscutting;
using Crosscutting.SellixPayload;
using MassTransit;

namespace DiscordSaga.Components.KafkaDto.Discord;

    public class LicenseGrantEvent : CorrelatedBy<Guid>
    {
        public SellixPayloadNormal.Root? Payload { get; set; }
        public Guid CorrelationId { get; set; }
    }

    public class LicenseNotificationEvent : CorrelatedBy<Guid>
    {
        public SellixPayloadNormal.Root? Payload { get; init; }
        public int? Quantity { get; init; }
        public DateTime Time { get; init; }
        public WhichSpec? WhichSpec { get; init; }
        public Guid CorrelationId { get; set; }
    }

