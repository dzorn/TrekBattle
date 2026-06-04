using Microsoft.EntityFrameworkCore;

namespace TrekBattle.Api.Data;

public sealed class TrekBattleDbContext : DbContext
{
    public TrekBattleDbContext(DbContextOptions<TrekBattleDbContext> options)
        : base(options)
    {
    }

    public DbSet<GameSessionEntity> GameSessions => Set<GameSessionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameSessionEntity>(entity =>
        {
            entity.ToTable("GameSessions");
            entity.HasKey(x => x.SessionId);

            entity.Property(x => x.PlayerName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ShipName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ResumeCode)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(x => x.ResumeCodeNormalized)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(x => x.StateJson)
                .IsRequired();

            entity.Property(x => x.CreatedUtc)
                .IsRequired();

            entity.Property(x => x.UpdatedUtc)
                .IsRequired();

            entity.HasIndex(x => x.ResumeCodeNormalized)
                .IsUnique();
        });
    }
}
