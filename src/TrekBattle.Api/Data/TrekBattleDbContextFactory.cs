using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TrekBattle.Api.Data;

public sealed class TrekBattleDbContextFactory : IDesignTimeDbContextFactory<TrekBattleDbContext>
{
    public TrekBattleDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TrekBattleDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=TrekBattle.DesignTime;Trusted_Connection=True;TrustServerCertificate=True");

        return new TrekBattleDbContext(optionsBuilder.Options);
    }
}
