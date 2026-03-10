using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            // Register Unit of Work as Scoped (one per request)
            services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();

            return services;
        }

        public static IServiceCollection AddEFInfrastructure<TContext, TUnitOfWork>(
            this IServiceCollection services)
            where TContext : DbContext
            where TUnitOfWork : class, IUnitOfWork
        {
            // Register custom Unit of Work implementation
            services.AddScoped<IUnitOfWork, TUnitOfWork>();

            return services;
        }
    }
}