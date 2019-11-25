using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace CosmosDBCacheLib
{
    public static class CosmosDbCacheExtensions
    {
        public static IServiceCollection AddDistributedCosmosDbCache(this IServiceCollection services, Action<CosmosDbCacheOptions> options)
        {
            OptionsServiceCollectionExtensions.AddOptions(services);
            OptionsServiceCollectionExtensions.Configure<CosmosDbCacheOptions>(services, options);

            services.AddSingleton<IDistributedCache, CosmosDbCache>();

            return services;
        }
    }
}
