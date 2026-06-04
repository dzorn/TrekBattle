namespace TrekBattle.Api.Contracts;

public sealed record StartGameRequest(string PlayerName, string ShipName);

public sealed record ResumeGameRequest(string ResumeCode);

public sealed record GameSessionState(
    Guid SessionId,
    string PlayerName,
    string ShipName,
    string ResumeCode,
    string MissionTitle,
    string MissionBrief,
    string MissionObjective,
    string CurrentScreen,
    DateTimeOffset CreatedUtc);
