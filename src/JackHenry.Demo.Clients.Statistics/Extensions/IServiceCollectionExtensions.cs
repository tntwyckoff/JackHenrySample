using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackHenry.Demo.Clients.Statistics.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddStatisticServiceClient(this IServiceCollection context, IConfiguration configuration)
        {
            var sConfig = new StatisticsServiceClientConfiguration();
            configuration.Bind("StatisticsServiceClientConfiguration", sConfig);

            return context.AddSingleton<StatisticsServiceClientConfiguration>(sConfig).AddSingleton<StatisticsServiceClient>();
        }
    }
}
