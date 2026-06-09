using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TrekBattle.Api.Data;

public sealed class TrekBattleDbSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<TrekBattleDbSaveChangesInterceptor> _logger;

    public TrekBattleDbSaveChangesInterceptor(ILogger<TrekBattleDbSaveChangesInterceptor> logger)
    {
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        LogPendingChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        LogPendingChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        _logger.LogInformation(
            "Database save completed for {ContextType}; {Rows} row(s) written.",
            eventData.Context?.GetType().Name ?? "unknown context",
            result);

        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Database save completed for {ContextType}; {Rows} row(s) written.",
            eventData.Context?.GetType().Name ?? "unknown context",
            result);

        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _logger.LogError(
            eventData.Exception,
            "Database save failed for {ContextType}.",
            eventData.Context?.GetType().Name ?? "unknown context");

        base.SaveChangesFailed(eventData);
    }

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        _logger.LogError(
            eventData.Exception,
            "Database save failed for {ContextType}.",
            eventData.Context?.GetType().Name ?? "unknown context");

        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    private void LogPendingChanges(DbContext? context)
    {
        if (context is null)
        {
            _logger.LogWarning("Database save started without a DbContext instance.");
            return;
        }

        var trackedEntries = context.ChangeTracker.Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Select(entry => $"{entry.Metadata.ClrType.Name}:{entry.State}")
            .ToArray();

        _logger.LogInformation(
            "Database save started for {ContextType} with {Count} tracked change(s): {Changes}.",
            context.GetType().Name,
            trackedEntries.Length,
            trackedEntries.Length == 0 ? "none" : string.Join(", ", trackedEntries));
    }
}
