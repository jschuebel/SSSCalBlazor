using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;


namespace SSSCalBlazor.Models
{
    public static class ServiceInjection
    {
        public static IServiceCollection AddMYServices(this IServiceCollection services)//, string baseadd)
        {
            //, NavigationManager navigationManager
            //services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(navigationManager.BaseUri) });
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IAddressService, AddressService>();

            return services;
        }
    }
}
