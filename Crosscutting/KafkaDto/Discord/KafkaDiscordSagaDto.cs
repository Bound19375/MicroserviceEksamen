using Crosscutting.SellixPayload;

namespace Crosscutting.KafkaDto.Discord;

public record KafkaDiscordSagaMessageDto
{
    public SellixPayloadNormal.Root? Payload { get; init; }
    public int? Quantity { get; init; }
    public DateTime Time { get; init; }
    public WhichSpec? WhichSpec { get; init; }
}

