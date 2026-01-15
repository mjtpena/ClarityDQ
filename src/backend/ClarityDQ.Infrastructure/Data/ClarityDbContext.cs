using ClarityDQ.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClarityDQ.Infrastructure.Data;

public class ClarityDbContext : DbContext
{
    public ClarityDbContext(DbContextOptions<ClarityDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<DataProfile> DataProfiles => Set<DataProfile>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<RuleExecution> RuleExecutions => Set<RuleExecution>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EntraIdObjectId).IsUnique();
            entity.HasIndex(e => e.Email);
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.EntraIdObjectId).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<DataProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorkspaceId, e.DatasetName, e.TableName });
            entity.HasIndex(e => e.ProfiledAt);
            entity.Property(e => e.WorkspaceId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DatasetName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.TableName).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.WorkspaceId, e.DatasetName, e.TableName });
            entity.HasIndex(e => e.IsEnabled);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.WorkspaceId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DatasetName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.TableName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ColumnName).HasMaxLength(255);
            entity.Property(e => e.CreatedBy).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<RuleExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RuleId);
            entity.HasIndex(e => e.ExecutedAt);
            entity.HasOne(e => e.Rule)
                .WithMany()
                .HasForeignKey(e => e.RuleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
