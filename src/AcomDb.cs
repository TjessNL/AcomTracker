namespace src;
using Microsoft.EntityFrameworkCore;

public class AcomDb : DbContext
{
    public AcomDb(DbContextOptions<AcomDb> options) : base(options) {}
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
}