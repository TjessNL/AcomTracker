namespace AcomTracker.Application.Services;

using AcomTracker.Application.DTOs;

public interface IExcelImportService
{
    ExcelPreviewDto Preview(Stream fileStream);
    Task<ImportResultDto> ConfirmAsync(ConfirmImportDto dto);
}
