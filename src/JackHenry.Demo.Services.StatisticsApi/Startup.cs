using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JackHenry.Demo.Clients.Statistics;
using JackHenry.Demo.Clients.Statistics.Extensions;
using JackHenry.Demo.Clients.TwitterApi;
using JackHenry.Demo.Clients.TwitterApi.Extensions;
using JackHenry.Demo.Services.StatisticsApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace JackHenry.Demo.Services.StatisticsApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddStatisticServiceClient(Configuration);
            services.AddTwitterSampleStreamClient(Configuration);
            services.AddHostedService<TwitterMonitorService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
        }
    }
}
