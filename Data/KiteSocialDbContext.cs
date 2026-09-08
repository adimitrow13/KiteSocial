using KiteSocial.API.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KiteSocial.API.Data;

public class KiteSocialDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public KiteSocialDbContext(DbContextOptions<KiteSocialDbContext> options) : base(options)
    {
    }

    public DbSet<Spot> Spots => Set<Spot>();
    public DbSet<KiteSession> KiteSessions => Set<KiteSession>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Like> Likes => Set<Like>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser configurations
        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.FullName).HasMaxLength(100);
            b.Property(u => u.Bio).HasMaxLength(500);

            b.HasOne(u => u.HomeSpot)
                .WithMany()
                .HasForeignKey(u => u.HomeSpotId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Spot configurations
        builder.Entity<Spot>(b =>
        {
            b.Property(s => s.Name).IsRequired().HasMaxLength(150);
            b.Property(s => s.Country).IsRequired().HasMaxLength(100);
            b.Property(s => s.Region).HasMaxLength(100);
            b.Property(s => s.BestWindDirections).HasMaxLength(100);

            b.HasOne(s => s.CreatedBy)
                .WithMany()
                .HasForeignKey(s => s.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // KiteSession configurations
        builder.Entity<KiteSession>(b =>
        {
            b.HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(s => s.Spot)
                .WithMany(sp => sp.Sessions)
                .HasForeignKey(s => s.SpotId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Post configurations
        builder.Entity<Post>(b =>
        {
            b.Property(p => p.Content).IsRequired().HasMaxLength(2000);

            b.HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(p => p.Spot)
                .WithMany(s => s.Posts)
                .HasForeignKey(p => p.SpotId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(p => p.KiteSession)
                .WithMany(s => s.Posts)
                .HasForeignKey(p => p.KiteSessionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Comment configurations
        builder.Entity<Comment>(b =>
        {
            b.Property(c => c.Content).IsRequired().HasMaxLength(1000);

            b.HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Like configurations (one like per user per post)
        builder.Entity<Like>(b =>
        {
            b.HasIndex(l => new { l.PostId, l.UserId }).IsUnique();

            b.HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
