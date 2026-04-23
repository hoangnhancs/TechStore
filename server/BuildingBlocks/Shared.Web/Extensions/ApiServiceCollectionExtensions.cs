using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Web.Extensions
{
    public static class ApiServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedControllers(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(x =>
                    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            return services;
        }
    }
}