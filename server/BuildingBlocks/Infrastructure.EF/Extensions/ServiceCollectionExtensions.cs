using Infrastructure.EF.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.EF.UnitOfWork;

namespace Infrastructure.EF.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEFInfrastructure<TContext>(
            this IServiceCollection services)
            where TContext : DbContext
        {
            // Register the default EF unit of work implementation.
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();

            return services;
        }

        public static IServiceCollection AddEFInfrastructure<TContext, TUnitOfWork>(
            this IServiceCollection services)
            where TContext : DbContext
            where TUnitOfWork : class, IUnitOfWork
        {
            // Register a custom unit of work as the shared abstraction.
            services.AddScoped<IUnitOfWork, TUnitOfWork>();

            return services;
        }

        // public static IServiceCollection AddEFInfrastructure<TContext, TAbstraction, TUnitOfWork>(
        //     this IServiceCollection services)
        //     where TContext : DbContext
        //     where TAbstraction : class, IUnitOfWork
        //     where TUnitOfWork : class, TAbstraction
        // {
        //     // Register domain-specific abstraction and map IUnitOfWork to the same scoped instance.
        //     services.AddScoped<TAbstraction, TUnitOfWork>();
        //     services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TAbstraction>());

        //     return services;
        // }
    }
}