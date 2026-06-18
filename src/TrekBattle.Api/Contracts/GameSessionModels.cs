namespace TrekBattle.Api.Contracts;

public sealed record StartGameRequest(string PlayerName, string ShipName);

public sealed record ResumeGameRequest(string ResumeCode);

public sealed record WarpJumpRequest(int DestinationX, int DestinationY);

public sealed record GalaxySectorState(
    int X,
    int Y,
    int EnemyCount,
    int PlanetCount,
    int BaseCount,
    bool Visited,
    bool Scanned);

public sealed record GalaxyMapState(
    int Width,
    int Height,
    int JumpRange,
    int CurrentX,
    int CurrentY,
    int CompletedTurns,
    bool ActionUsed,
    bool MovementUsed,
    IReadOnlyList<GalaxySectorState> Sectors);

public sealed record GameSessionState(
    Guid SessionId,
    string PlayerName,
    string ShipName,
    string ResumeCode,
    string MissionTitle,
    string MissionBrief,
    string MissionObjective,
    string CurrentScreen,
    DateTimeOffset CreatedUtc,
    GalaxyMapState GalaxyMap);
