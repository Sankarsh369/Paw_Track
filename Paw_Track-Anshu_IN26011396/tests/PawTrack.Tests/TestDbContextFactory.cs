using Microsoft.EntityFrameworkCore;
using PawTrack.Data;

namespace PawTrack.Tests;

public static class TestDbContextFactory
{
    public static PawTrackDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<PawTrackDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new PawTrackDbContext(options);
        DbInitializer.Initialize(context);
        return context;
    }
}
