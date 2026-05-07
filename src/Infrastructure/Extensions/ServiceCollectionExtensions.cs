namespace AcomTracker.Infrastructure.Extensions;

using AcomTracker.Application.Services;
using AcomTracker.Domain.Interfaces;
using AcomTracker.Infrastructure.Persistence;
using AcomTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AcomDb>(opt =>
            opt.UseNpgsql(config.GetConnectionString("AcomDb")));

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IExcelImportService, ExcelImportService>();

        return services;
    }
}
