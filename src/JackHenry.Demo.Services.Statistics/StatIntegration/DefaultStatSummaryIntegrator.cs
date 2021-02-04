using m = JackHenry.Demo.Libraries.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JackHenry.Demo.Services.Statistics.StatIntegration
{
    public class DefaultStatSummaryIntegrator : IStatSummaryIntegrator
    {

        public m.TwitterStatSummary IntegrateSummaryData(m.TwitterStatSummary data1, m.TwitterStatSummary data2)
        {
            var result = new m.TwitterStatSummary();

            if (0 == data1.FirstTweetTime)
                result.FirstTweetTime = data2.FirstTweetTime;
            else if (0 == data2.FirstTweetTime)
                result.FirstTweetTime = data1.FirstTweetTime;
            else
                result.FirstTweetTime = Math.Min(data1.FirstTweetTime, data2.FirstTweetTime);

            result.LastTweetTime = Math.Max(data1.LastTweetTime, data2.LastTweetTime);
            result.NumberOfTweets = data1.NumberOfTweets + data2.NumberOfTweets;
            result.NumberEmojiTweets = data1.NumberEmojiTweets + data2.NumberEmojiTweets;
            result.NumberOfUrlTweets = data1.NumberOfUrlTweets + data2.NumberOfUrlTweets;
            result.NumberOfPhotoUrlTweets = data1.NumberOfPhotoUrlTweets + data2.NumberOfPhotoUrlTweets;
            result.HashtagCount = mergeCountDictionaries(data1.HashtagCount, data2.HashtagCount);
            result.DomainCount = mergeCountDictionaries(data1.DomainCount, data2.DomainCount);
            result.EmojiCount = mergeCountDictionaries(data1.EmojiCount, data2.EmojiCount);

            return result;
        }


        private Dictionary<string, int> mergeCountDictionaries(IDictionary<string, int> dictionary1, IDictionary<string, int> dictionary2)
        {
            if (null == dictionary1)
                return dictionary2 as Dictionary<string, int>;

            var result = new Dictionary<string, int>();

            foreach (var kvp in dictionary1)
            {
                var val = kvp.Value;

                int val2;

                if (dictionary2.TryGetValue(kvp.Key, out val2))
                {
                    val += val2;
                }

                result[kvp.Key] = val;
            }

            foreach (var kvp in dictionary2)
            {
                if (result.ContainsKey(kvp.Key))
                    continue;

                result[kvp.Key] = kvp.Value;
            }

            return result;
        }

    }
}