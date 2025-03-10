using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JackHenry.Demo.Services.StatisticsApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JackHenry.Demo.Services.StatisticsApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                //.ConfigureServices(services =>
                //{
                //    services.AddHostedService<TwitterMonitorService>();
                //})
                ;
    }
}
