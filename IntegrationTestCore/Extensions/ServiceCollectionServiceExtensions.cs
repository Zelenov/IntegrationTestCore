using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace IntegrationTestCore
{
    public static class ServiceCollectionServiceExtensions
    {
        public static IServiceCollection Remove(this IServiceCollection services, Type serviceType)
        {
            var service = services.Where(s => s.ServiceType == serviceType).ToArray();
            foreach (var serviceDescriptor in service)
            {
                services.Remove(serviceDescriptor);
            }

            return services;
        }
        public static IServiceCollection RemoveHostedServices(this IServiceCollection services)
        {
            return services.Remove(typeof(IHostedService));
        }

        public static IServiceCollection WithOptions<T>(this IServiceCollection services, T options) where T : class, new()
        {
            IOptions<T> o = new OptionsWrapper<T>(options);
            services.AddTransient(p => o);
            return services;
        }
    }
}
