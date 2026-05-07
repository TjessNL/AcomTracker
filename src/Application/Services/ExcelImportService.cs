namespace AcomTracker.Application.Services;

using AcomTracker.Application.DTOs;
using AcomTracker.Domain.Entities;
using AcomTracker.Domain.Interfaces;
using ClosedXML.Excel;

public class ExcelImportService(ITenantRepository tenantRepo, IPaymentRepository paymentRepo, IExpenseRepository expenseRepo) : IExcelImportService
{
    public ExcelPreviewDto Preview(Stream fileStream)
    {
        using var wb = new XLWorkbook(fileStream);
        var ws = wb.Worksheets.First();

        var tenants  = new List<ExcelTenantRow>();
        var expenses = new List<ExcelExpenseRow>();
        var issues   = new List<ImportIssue>();

        // Row 1 is the header row — extract payment date columns (cols D onward up to the balance col)
        var paymentDates = ParseHeaderDates(ws, issues);

        // Rows 2+ until we hit a non-tenant row (row number in col A or blank name)
        int row = 2;
        while (row <= ws.LastRowUsed()?.RowNumber())
        {
            var cell = ws.Cell(row, 1);
            var colA = CellText(cell);

            // If col A is a row number digit, this is a tenant row
            if (!int.TryParse(colA, out var rowNum))
            {
                row++;
                continue;
            }

            var nameCell = ws.Cell(row, 2);
            var name = CellText(nameCell).Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                row++;
                continue;
            }

            // Col C = monthly rent
            var rentRaw = CellText(ws.Cell(row, 3));
            decimal rent = 0;
            if (!TryParseDecimal(rentRaw, out rent))
                issues.Add(new ImportIssue("Tenant", rowNum, "MonthlyRent", $"Could not parse rent '{rentRaw}'", rentRaw));

            var tenantIssues = new List<string>();

            // Suspiciously low rent flag (less than 1000 — currencies like NGN have rents in 100k range)
            if (rent > 0 && rent < 1_000)
                tenantIssues.Add($"Monthly rent {rent:N0} looks suspiciously low — did you mean {rent * 1000:N0}?");

            // Payment columns start at col D (index 4)
            var payments = new List<ExcelPaymentRow>();
            for (int di = 0; di < paymentDates.Count; di++)
            {
                var col     = 4 + di;
                var raw     = CellText(ws.Cell(row, col));
                if (string.IsNullOrWhiteSpace(raw)) continue;
                if (!TryParseDecimal(raw, out var amt) || amt <= 0) continue;
                payments.Add(new ExcelPaymentRow(paymentDates[di], amt));
            }

            // Outstanding balance — last meaningful column after payments
            // Try to find the column labelled "OUT STANDING BALANCE" or similar from header
            var balanceColIdx = paymentDates.Count + 4; // col right after payment columns
            var balanceRaw = CellText(ws.Cell(row, balanceColIdx));

            // Detect "NIL", "NILL", blank = 0 outstanding
            if (IsNilText(balanceRaw)) balanceRaw = "0";

            tenants.Add(new ExcelTenantRow(rowNum, name, rent, payments, balanceRaw, tenantIssues));
            row++;
        }

        // Expense section — look for rows where col A has a description and col G has an amount
        // These appear after the tenant block
        for (int r = row; r <= (ws.LastRowUsed()?.RowNumber() ?? 0); r++)
        {
            var descRaw = CellText(ws.Cell(r, 1)).Trim();
            if (string.IsNullOrWhiteSpace(descRaw)) continue;

            // Skip summary rows
            if (descRaw.Contains("TOTAL", StringComparison.OrdinalIgnoreCase) ||
                descRaw.Contains("DEDUCT", StringComparison.OrdinalIgnoreCase) ||
                descRaw.Contains("CASH", StringComparison.OrdinalIgnoreCase)) continue;

            // Try col G (index 7) for amount
            var amtRaw = CellText(ws.Cell(r, 7)).Trim();
            if (!TryParseDecimal(amtRaw, out var expAmt) || expAmt <= 0) continue;

            var expIssues = new List<string>();
            if (expAmt < 100)
                expIssues.Add($"Expense amount {expAmt:N0} is unusually low");

            expenses.Add(new ExcelExpenseRow(descRaw, expAmt, expIssues));
        }

        // Collect all per-row issues into the top-level list
        foreach (var t in tenants)
            foreach (var msg in t.Issues)
                issues.Add(new ImportIssue("Tenant", t.RowNumber, "general", msg));

        foreach (var e in expenses)
            foreach (var msg in e.Issues)
                issues.Add(new ImportIssue("Expense", 0, "general", msg));

        return new ExcelPreviewDto(tenants, expenses, issues);
    }

    public async Task<ImportResultDto> ConfirmAsync(ConfirmImportDto dto)
    {
        int tenantsImported = 0, paymentsImported = 0, expensesImported = 0;

        foreach (var row in dto.Tenants)
        {
            var name = row.Name.Trim();
            if (string.IsNullOrWhiteSpace(name)) continue;

            // Skip if tenant already exists by name
            if (await tenantRepo.ExistsByNameAsync(name)) continue;

            var tenant = new Tenant
            {
                Name           = name,
                MonthlyRent    = row.MonthlyRent,
                LeaseStartDate = dto.DefaultLeaseStart,
                IsActive       = true,
                Payments       = [],
                Leases         = []
            };
            await tenantRepo.AddAsync(tenant);
            await tenantRepo.SaveChangesAsync();
            tenantsImported++;

            foreach (var p in row.Payments)
            {
                await paymentRepo.AddAsync(new Payment
                {
                    TenantId = tenant.TenantId,
                    Amount   = p.Amount,
                    Date     = p.Date,
                    Method   = "Cash",
                    Notes    = "Imported from Excel"
                });
                paymentsImported++;
            }
            await paymentRepo.SaveChangesAsync();
        }

        foreach (var row in dto.Expenses)
        {
            if (string.IsNullOrWhiteSpace(row.Description) || row.Amount <= 0) continue;
            await expenseRepo.AddAsync(new Expense
            {
                Description = row.Description.Trim(),
                Amount      = row.Amount,
                Date        = dto.DefaultLeaseStart,
                Category    = "Imported",
                CreatedAt   = DateTime.UtcNow
            });
            expensesImported++;
        }
        if (expensesImported > 0)
            await expenseRepo.SaveChangesAsync();

        return new ImportResultDto(tenantsImported, paymentsImported, expensesImported);
    }

    // ── Helpers ───────────────────────────────────────────────

    private static List<DateOnly> ParseHeaderDates(IXLWorksheet ws, List<ImportIssue> issues)
    {
        var dates = new List<DateOnly>();
        // Payment date columns: D, E, F, G, H (cols 4-8)
        for (int col = 4; col <= 8; col++)
        {
            var cell = ws.Cell(1, col);
            if (cell.DataType == XLDataType.DateTime)
            {
                dates.Add(DateOnly.FromDateTime(cell.GetDateTime()));
            }
            else
            {
                var raw = CellText(cell).Trim();
                if (TryParseDate(raw, out var d))
                    dates.Add(d);
                else if (!string.IsNullOrWhiteSpace(raw))
                    issues.Add(new ImportIssue("Header", 1, $"Col{col}", $"Could not parse date '{raw}'", raw));
            }
        }
        return dates;
    }

    private static string CellText(IXLCell cell)
    {
        if (cell.IsEmpty()) return "";
        return cell.DataType switch
        {
            XLDataType.DateTime => cell.GetDateTime().ToString("yyyy-MM-dd"),
            XLDataType.Number   => cell.GetDouble().ToString(System.Globalization.CultureInfo.InvariantCulture),
            _                   => cell.GetString()
        };
    }

    private static bool TryParseDecimal(string raw, out decimal value)
    {
        value = 0;
        var cleaned = raw.Replace(",", "").Trim();
        return decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseDate(string raw, out DateOnly date)
    {
        date = default;
        // Handle typos like "31/08/025" → "31/08/2025"
        var cleaned = System.Text.RegularExpressions.Regex.Replace(raw, @"\b(\d{1,2})/(\d{1,2})/(\d{2,3})\b", m =>
        {
            var y = m.Groups[3].Value;
            if (y.Length == 2) y = "20" + y;
            else if (y.Length == 3) y = "2" + y;
            return $"{m.Groups[1].Value}/{m.Groups[2].Value}/{y}";
        });

        string[] formats = ["d/M/yyyy", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd"];
        return DateOnly.TryParseExact(cleaned, formats,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out date);
    }

    private static bool IsNilText(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return true;
        var u = raw.Trim().ToUpperInvariant();
        return u == "NIL" || u == "NILL" || u == "NILL " || u.StartsWith("NIL") || u.StartsWith("TILL");
    }
}
