namespace AcomTracker.Application.Services;

using AcomTracker.Application.DTOs;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseDto>> GetAllAsync(string? category = null);
    Task<ExpenseDto?> GetByIdAsync(int id);
    Task<ExpenseDto> CreateAsync(CreateExpenseDto dto, byte[]? photoData = null, string? photoMimeType = null);
    Task<bool> DeleteAsync(int id);
    Task<(byte[] Data, string MimeType)?> GetPhotoAsync(int id);
    Task<IEnumerable<string>> GetCategoriesAsync();
}
