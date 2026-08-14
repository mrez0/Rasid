using Microsoft.EntityFrameworkCore;
using Rasid.Core.Models;

namespace Rasid.Core.Data;

public class RasidDbContext : DbContext
{
    public RasidDbContext(DbContextOptions<RasidDbContext> options) : base(options)
    {
    }

    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<Video> Videos => Set<Video>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Channel>()
            .HasMany(c => c.Videos)
            .WithOne(v => v.Channel)
            .HasForeignKey(v => v.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Video>()
            .HasIndex(v => new { v.ChannelId, v.PublishedUtc });
    }
}