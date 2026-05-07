namespace AcomTracker.Application.Services;

using AcomTracker.Application.DTOs;
using AcomTracker.Domain.Entities;
using AcomTracker.Domain.Interfaces;

public class ExpenseService(IExpenseRepository repo) : IExpenseService
{
    private static ExpenseDto ToDto(Expense e) => new(
        e.ExpenseId, e.Description, e.Amount, e.Date,
        e.Category, e.Notes, e.PhotoData is not null, e.CreatedAt);

    public async Task<IEnumerable<ExpenseDto>> GetAllAsync(string? category = null)
    {
        var all = await repo.GetAllAsync();
        if (!string.IsNullOrWhiteSpace(category))
            all = all.Where(e => string.Equals(e.Category, category, StringComparison.OrdinalIgnoreCase));
        return all.Select(ToDto);
    }

    public async Task<ExpenseDto?> GetByIdAsync(int id)
    {
        var e = await repo.GetByIdAsync(id);
        return e is null ? null : ToDto(e);
    }

    public async Task<ExpenseDto> CreateAsync(CreateExpenseDto dto, byte[]? photoData = null, string? photoMimeType = null)
    {
        var expense = new Expense
        {
            Description   = dto.Description.Trim(),
            Amount        = dto.Amount,
            Date          = dto.Date,
            Category      = dto.Category?.Trim(),
            Notes         = dto.Notes?.Trim(),
            PhotoData     = photoData,
            PhotoMimeType = photoMimeType,
            CreatedAt     = DateTime.UtcNow
        };
        await repo.AddAsync(expense);
        await repo.SaveChangesAsync();
        return ToDto(expense);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var expense = await repo.GetByIdAsync(id);
        if (expense is null) return false;
        await repo.DeleteAsync(expense);
        await repo.SaveChangesAsync();
        return true;
    }

    public async Task<(byte[] Data, string MimeType)?> GetPhotoAsync(int id)
    {
        var expense = await repo.GetByIdAsync(id);
        if (expense?.PhotoData is null || expense.PhotoMimeType is null) return null;
        return (expense.PhotoData, expense.PhotoMimeType);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        var all = await repo.GetAllAsync();
        return all.Where(e => e.Category is not null)
                  .Select(e => e.Category!)
                  .Distinct()
                  .OrderBy(c => c);
    }
}
