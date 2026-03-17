public interface ITenantRepository
{
    Task<IEnumerable<Tenant>> GetAllAsync(string? search = null);
    Task<Tenant?> GetByIdAsync(int id, bool ignoreFilter = false);
    Task<bool> ExistsByNameAsync(string name);
    Task AddAsync(Tenant tenant);
    Task SaveChangesAsync();
}