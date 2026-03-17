namespace AcomTracker.Infrastructure.Repositories;

using AcomTracker.Domain.Entities;
using AcomTracker.Domain.Interfaces;
using AcomTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class TenantRepository(AcomDb db) : ITenantRepository
{
    public async Task<IEnumerable<Tenant>> GetAllAsync(string? search = null)
    {
        var query = db.Tenants
            .Where(t => t.IsActive)
            .Include(t => t.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.ToLower().Contains(search.ToLower()));

        return await query.ToListAsync();
    }

    public async Task<Tenant?> GetByIdAsync(int id, bool ignoreFilter = false)
    {
        var query = db.Tenants
            .Include(t => t.Payments)
            .AsQueryable();

        if (ignoreFilter)
            query = query.IgnoreQueryFilters();

        return await query.FirstOrDefaultAsync(t => t.TenantId == id);
    }

    public async Task<bool> ExistsByNameAsync(string name) =>
        await db.Tenants
            .AnyAsync(t => t.Name.ToLower() == name.ToLower());

    public async Task AddAsync(Tenant tenant) =>
        await db.Tenants.AddAsync(tenant);

    public async Task SaveChangesAsync() =>
        await db.SaveChangesAsync();
}