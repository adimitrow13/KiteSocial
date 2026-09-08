using Microsoft.EntityFrameworkCore;

namespace KiteSocial.API.Data;

public class KiteSocialDbContext : DbContext
{
    public KiteSocialDbContext(DbContextOptions<KiteSocialDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Entity configurations will be registered here
    }
}
