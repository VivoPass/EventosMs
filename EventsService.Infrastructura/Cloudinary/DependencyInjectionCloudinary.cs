using EventsService.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventsService.Infrastructura.Cloudinary
{

    public static class DependencyInjectionCloudinary   // ← DEBE SER static
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Bind de configuración
            var cloudinarySettings = new CloudinarySettings();
            configuration.GetSection("Cloudinary").Bind(cloudinarySettings);

            services.AddSingleton(cloudinarySettings);

            // Cliente de Cloudinary
            var account = new Account(
                cloudinarySettings.CloudName,
                cloudinarySettings.ApiKey,
                cloudinarySettings.ApiSecret);

            var cloudinary = new CloudinaryDotNet.Cloudinary(account);
            services.AddSingleton(cloudinary);

            // Servicio de almacenamiento
            services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();

            return services;
        }
    }

}
