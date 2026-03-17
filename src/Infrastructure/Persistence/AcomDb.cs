namespace AcomTracker.Infrastructure.Persistence;

using System.Dynamic;
using Microsoft.EntityFrameworkCore;

public class AcomDb : DbContext
{
    public AcomDb(DbContextOptions<AcomDb> options) : base(options) {}
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Lease> Leases => Set<Lease>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Lease>()
        .HasOne(l => l.Tenant)
        .WithMany(t => t.Leases)
        .HasForeignKey(l => l.TenantId)
        .OnDelete(DeleteBehavior.Restrict);

    modelBuilder.Entity<Payment>()
    .HasOne(p => p.Tenant)
    .WithMany(t => t.Payments)
    .HasForeignKey(p => p.TenantId)
    .OnDelete(DeleteBehavior.Restrict);


    modelBuilder.Entity<Tenant>()
        .HasQueryFilter(t => t.IsActive);
}
}