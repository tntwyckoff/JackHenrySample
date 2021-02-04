using JackHenry.Demo.Persistence.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackHenry.Demo.Libraries.ConcurrentAtomicDictionary.Extensions
{
    public static class IServiceCollectionExtensions
    {

        public static IServiceCollection UseHotSwappingAtomicDictionary(this IServiceCollection context, IConfiguration configuration)
        {
            var sConfig = new HotSwapOptions();
            configuration.Bind("HotSwapOptions", sConfig);

            return context.AddSingleton<HotSwapOptions>(sConfig).AddSingleton<IAtomicDictionary, HotSwappingReadWriteDictionary>();
        }

        public static IServiceCollection UseConcurrentAtomicDictionary(this IServiceCollection context)
        {
            return context.AddSingleton<IAtomicDictionary, ConcurrentAtomicDictionary>();
        }

    }
}
