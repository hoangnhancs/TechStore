using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotoService.Configs;
using PhotoService.Interface;
using PhotoService.Services;

namespace PhotoService.Extensions
{
    public static class ServiceCollectionExtensions // Phải là static class
    {
        // Extension method - tham số đầu tiên có "this"
        public static IServiceCollection AddPhotoServices(
            this IServiceCollection services,  // "this" = đây là extension method
            IConfiguration configuration)
        {
            var provider = configuration["PhotoStorage:Provider"] ?? "Cloudinary";

            if (string.Equals(provider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
            {
                services.Configure<AzureBlobSettings>(configuration.GetSection("AzureBlobSettings") ?? throw new InvalidOperationException("AzureBlobSettings section is missing in configuration"));
                services.AddScoped<IPhotoService, AzureBlobPhotoService>();
            }
            else
            {
                services.Configure<CloudinarySettings>(configuration.GetSection("CloudinarySettings") ?? throw new InvalidOperationException("CloudinarySettings section is missing in configuration"));
                services.AddScoped<ICloudinaryServices, CloudinaryServices>();
                services.AddScoped<IPhotoService, PhotoService>();
            }

            return services; // Return để có thể chain methods
        }
    }
}