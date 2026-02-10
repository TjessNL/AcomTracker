namespace src;
using Microsoft.EntityFrameworkCore;

public class AcomDb : DbContext
{
    public AcomDb(DbContextOptions<AcomDb> options) : base(options) {}
    public DbSet<Expense> expenses => Set<Expense>();
    public DbSet<Payment> payments => Set<Payment>();
    public DbSet<Tenant> tenants => Set<tenant>();
}