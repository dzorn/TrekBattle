namespace TrekBattle.Api.Data;

public sealed class GameSessionEntity
{
    public Guid SessionId { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public string ShipName { get; set; } = string.Empty;

    public string ResumeCode { get; set; } = string.Empty;

    public string ResumeCodeNormalized { get; set; } = string.Empty;

    public string StateJson { get; set; } = string.Empty;

    public DateTimeOffset CreatedUtc { get; set; }

    public DateTimeOffset UpdatedUtc { get; set; }
}
