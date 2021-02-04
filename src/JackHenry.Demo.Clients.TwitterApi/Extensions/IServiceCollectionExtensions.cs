using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackHenry.Demo.Clients.TwitterApi.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddTwitterSampleStreamClient(this IServiceCollection context, IConfiguration configuration)
        {
            var sConfig = new TwitterClientConfiguration();
            configuration.Bind("TwitterClientConfiguration", sConfig);

            return context.AddSingleton<TwitterClientConfiguration>(sConfig)
                            .AddSingleton<TwitterSampleStreamPump>();
        }
    }
}
