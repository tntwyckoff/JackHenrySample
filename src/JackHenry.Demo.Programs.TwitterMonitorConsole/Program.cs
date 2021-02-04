using JackHenry.Demo.Clients.Statistics;
using JackHenry.Demo.Clients.Statistics.Extensions;
using JackHenry.Demo.Clients.TwitterApi;
using JackHenry.Demo.Clients.TwitterApi.ApiModels;
using JackHenry.Demo.Clients.TwitterApi.Extensions;
using JackHenry.Demo.Libraries.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JackHenry.Demo.Programs.TwitterMonitorConsole
{
    class Program
    {
        static IServiceProvider Services;
        static IConfigurationRoot Configuration;
        static StatisticsServiceClient _statsClient;


        static void Main(string[] args)
        {
            IHost host = createHostBuilder(args).Build();
            Services = host.Services;

            var cancellationTokenSource = new CancellationTokenSource();

            _statsClient = Services.GetRequiredService<StatisticsServiceClient>();
            var twitterClient = Services.GetRequiredService<TwitterSampleStreamPump>();

            Console.WriteLine("Connecting to Twitter API. Press <Enter> to quit.");

            _ = twitterClient.DownloadSampleStreamChunked(handleBatch, cancellationTokenSource.Token, 30);

            Console.ReadLine();
            cancellationTokenSource.Cancel();
        }


        static void handleBatch(List<Tweet> tweets)
        {
            Task.Run(() => backgroundBatch(tweets));
        }

        static async Task backgroundBatch(List<Tweet> tweets)
        {
            var statsSummary = computeSummary(tweets);

            _ = await _statsClient.PostStatisticSummaryAsync(statsSummary);
        }

        static TwitterStatSummary computeSummary(List<Tweet> tweets)
        {
            var result = new TwitterStatSummary();

            var minTime = tweets.AsParallel().Min(t => t.CreatedDateAsDate);
            var maxTime = tweets.AsParallel().Max(t => t.CreatedDateAsDate);

            var tagsGrouped = tweets.AsParallel()
                                    .Where(t => t.HasHashTag)
                                    .SelectMany(t => t.entities.hashtags.Select(ht => ht.tag))
                                    .GroupBy(t => t)
                                    .Select(g => new { Key = g.Key, Count = g.Count() });
            var urlsGrouped = tweets.AsParallel()
                                    .Where(t => t.HasUrl)
                                    .SelectMany(t => t.entities.urls.Select(u => new Uri(u.expanded_url).Host))
                                    .GroupBy(t => t)
                                    .Select(g => new { Key = g.Key, Count = g.Count() });

            result.FirstTweetTime = tweets.Min(t => t.CreatedDateAsDate.Ticks);
            result.LastTweetTime = tweets.Max(t => t.CreatedDateAsDate.Ticks);
            result.NumberOfTweets = tweets.Count();
            result.NumberOfUrlTweets = tweets.AsParallel().Count(t => t.HasUrl);
            result.NumberOfPhotoUrlTweets = tweets.AsParallel().Count(t => t.HasPhotoUrl);
            //result.NumberEmojiTweets = 
            result.HashtagCount = tagsGrouped.ToDictionary(a => a.Key, a => a.Count);
            result.DomainCount = urlsGrouped.ToDictionary(a => a.Key, a => a.Count);

            return result;
        }

        static IHostBuilder createHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                 .ConfigureAppConfiguration((hostingContext, config) =>
                 {
                     config.AddEnvironmentVariables();
                     config.AddJsonFile("appsettings.json", optional: true);
                     config.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);

                     if (args != null)
                     {
                         config.AddCommandLine(args);
                     }

                     Configuration = config.Build();
                 }
                 )
                 .ConfigureServices(internalConfigureServices)
                 .ConfigureLogging(lb =>
                 {
                     lb.ClearProviders();
                     lb.AddConsole();
                 });
        }

        static void internalConfigureServices(IServiceCollection services)
        {
            services.AddTwitterSampleStreamClient(Configuration);
            services.AddStatisticServiceClient(Configuration);
        }

    }
}
