using KiteSocial.API.Data;
using Microsoft.EntityFrameworkCore;

namespace KiteSocial.Tests.Common;

public static class TestDbContextFactory
{
    public static KiteSocialDbContext Create()
    {
        var options = new DbContextOptionsBuilder<KiteSocialDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new KiteSocialDbContext(options);
    }
}
