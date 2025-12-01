using AuthenticationWebApplication.Enteties;
using MFAWebApplication.Outbox;
using Microsoft.EntityFrameworkCore;

namespace MFAWebApplication.Context;

public class WriteDbContext : DbContext
{
    public WriteDbContext( DbContextOptions<WriteDbContext> options ) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>().HasKey(x => x.Id);

        modelBuilder.Entity<OutboxMessage>().HasIndex(x => new { x.ProcessedAt, x.CreatedAt })
               .HasFilter("\"ProcessedAt\" IS NULL")
               .HasDatabaseName("IX_Outbox_Pending");

        modelBuilder.Entity<OutboxMessage>().HasIndex(x => x.ProcessedAt)
               .HasFilter("\"ProcessedAt\" IS NOT NULL")
               .HasDatabaseName("IX_Outbox_Processed_At");
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<User>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ConcurrencyIndex++;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

}
