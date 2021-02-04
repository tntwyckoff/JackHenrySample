using Grpc.Core;
using Grpc.Net.Client;
using JackHenry.Demo.Services.Statistics;
using m = JackHenry.Demo.Libraries.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackHenry.Demo.Clients.Statistics
{
    public class StatisticsServiceClient : IDisposable
    {
        private StatisticsServiceClientConfiguration _config;
        private GrpcChannel _channel;

        // https://docs.microsoft.com/en-us/aspnet/core/grpc/client?view=aspnetcore-5.0
        //  Grpc channels are costly to instantiate but can be shared across many clients;
        //  clients are lightweight and should be short-lived
        protected GrpcChannel Channel
        {
            get
            {
                if (null == _channel)
                    _channel = GrpcChannel.ForAddress(_config.ServiceAddress);

                return _channel;
            }
        }


        public StatisticsServiceClient(StatisticsServiceClientConfiguration config)
        {
            _config = config;
        }


        public async Task<bool> PostStatisticSummaryAsync(m.TwitterStatSummary data)
        {
            var response = await getStatClient().PostStatSummaryAsync(getServiceSummaryMessage(data));
            return response.Success;
        }

        public async Task<m.TwitterStatSummary> GetLatestStats()
        {
            var response = await getStatClient().GetLatestStatsAsync(new NullMessage());
            return getModelSummaryMessage(response);
        }


        // see note above
        StatSummary.StatSummaryClient getStatClient()
        {
            return new StatSummary.StatSummaryClient(Channel);
        }

        TwitterStatSummary getServiceSummaryMessage(m.TwitterStatSummary model)
        {
            var result = new TwitterStatSummary
            {
                FirstTweetTime = model.FirstTweetTime,
                LastTweetTime = model.LastTweetTime,
                NumberOfTweets = model.NumberOfTweets,
                NumberOfUrlTweets = model.NumberOfUrlTweets,
                NumberOfPhotoUrlTweets = model.NumberOfPhotoUrlTweets,
                NumberEmojiTweets = model.NumberEmojiTweets,
            };

            foreach (var kvp in model.HashtagCount)
                result.HashtagCount.Add(kvp.Key, kvp.Value);

            foreach (var kvp in model.DomainCount)
                result.DomainCount.Add(kvp.Key, kvp.Value);

            return result;
        }

        m.TwitterStatSummary getModelSummaryMessage(TwitterStatSummary data)
        {
            var result = new m.TwitterStatSummary
            {
                FirstTweetTime = data.FirstTweetTime,
                LastTweetTime = data.LastTweetTime,
                NumberOfTweets = data.NumberOfTweets,
                NumberOfUrlTweets = data.NumberOfUrlTweets,
                NumberOfPhotoUrlTweets = data.NumberOfPhotoUrlTweets,
                NumberEmojiTweets = data.NumberEmojiTweets,
            };

            data.DomainCount.ToList().ForEach(kvp => result.DomainCount.Add(kvp.Key, kvp.Value));
            data.HashtagCount.ToList().ForEach(kvp => result.HashtagCount.Add(kvp.Key, kvp.Value));

            return result;
        }


        public void Dispose()
        {
            if (null == _channel)
                return;

            _channel.Dispose();
            _channel = null;
        }

    }
}
