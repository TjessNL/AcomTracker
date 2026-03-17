namespace AcomTracker.Infrastructure.Repositories;

using AcomTracker.Domain.Entities;
using AcomTracker.Domain.Interfaces;
using AcomTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ExpenseRepository(AcomDb db) : IExpenseRepository
{
    public async Task<IEnumerable<Expense>> GetAllAsync() =>
        await db.Expenses
            .OrderByDescending(e => e.Date)
            .ToListAsync();

    public async Task<Expense?> GetByIdAsync(int id) =>
        await db.Expenses.FindAsync(id);

    public async Task AddAsync(Expense expense) =>
        await db.Expenses.AddAsync(expense);

    public async Task DeleteAsync(Expense expense) =>
        db.Expenses.Remove(expense);

    public async Task SaveChangesAsync() =>
        await db.SaveChangesAsync();
}