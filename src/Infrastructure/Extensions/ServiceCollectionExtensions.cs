namespace AcomTracker.Infrastructure.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AcomDb>(opt =>
            opt.UseNpgsql(config.GetConnectionString("AcomDb")));

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}