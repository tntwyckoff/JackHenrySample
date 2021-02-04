using JackHenry.Demo.Clients.Statistics;
using JackHenry.Demo.Clients.TwitterApi;
using JackHenry.Demo.Clients.TwitterApi.ApiModels;
using JackHenry.Demo.Libraries.Models;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JackHenry.Demo.Services.StatisticsApi.Services
{
    /*
     * The priority here is to prevent any blocking between receive and push
     * because there's tweets/second metric, we need to stop/start a stopwatch 
     * 
     */


    public class TwitterMonitorService : BackgroundService
    {
        private TwitterSampleStreamPump _twitterStream;
        private StatisticsServiceClient _statsClient;
        private object _batchLock = new object();


        public TwitterMonitorService(TwitterSampleStreamPump twitterStream, StatisticsServiceClient statsClient)
        {
            _twitterStream = twitterStream;
            _statsClient = statsClient;
        }


        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _twitterStream.DownloadSampleStreamChunked(handleBatch, cancellationToken);
        }


        void handleBatch(List<Tweet> tweets)
        {
            lock (_batchLock)
            {
                Task.Run(() => backgroundBatch(tweets));
            }
        }

        async Task backgroundBatch(List<Tweet> tweets)
        {
            var statsSummary = computeSummary(tweets);

            _ = await _statsClient.PostStatisticSummaryAsync(statsSummary);
        }

        TwitterStatSummary computeSummary(List<Tweet> tweets)
        {
            var result = new TwitterStatSummary();

            var minTime = tweets.AsParallel().Min(t => t.CreatedDateAsDate);
            var maxTime = tweets.AsParallel().Max(t => t.CreatedDateAsDate);

            var tagsGrouped = tweets.AsParallel()
                                    .Where(t => t.HasHashTag)
                                    .SelectMany(t => t.entities.hashtags.Select(ht => ht.tag.ToLower()))
                                    .GroupBy(t => t)
                                    .Select(g => new { Key = g.Key, Count = g.Count() });
            var urlsGrouped = tweets.AsParallel()
                                    .Where(t => t.HasUrl)
                                    .SelectMany(t => t.entities.urls.Select(u => new Uri(u.expanded_url).Host.ToLower()))
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

    }
}
