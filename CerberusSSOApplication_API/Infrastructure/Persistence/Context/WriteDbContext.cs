using Domain.Entities;
using Domain.Entities.Client;
using Domain.Entities.User;
using Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Context;

public class WriteDbContext : DbContext
{
    public WriteDbContext(DbContextOptions<WriteDbContext> options) : base(options) { }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<User> Users { get; set; }
    public DbSet<Client> Clients { get; set; }
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
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            Console.WriteLine($"{entry.Entity.GetType().Name} - {entry.State}");
            if (entry.State == EntityState.Added)
            {
                entry.Property(x => x.CreateDate).CurrentValue = utcNow;
                entry.Property(x => x.UpdateDate).CurrentValue = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.ConcurrencyIndex).CurrentValue =
                    entry.Entity.ConcurrencyIndex + 1;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

}
