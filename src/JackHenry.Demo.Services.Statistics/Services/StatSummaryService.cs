using m = JackHenry.Demo.Libraries.Models;
using JackHenry.Demo.Services.Statistics.StatIntegration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using System.Collections.Concurrent;
using JackHenry.Demo.Persistence.Abstractions;

namespace JackHenry.Demo.Services.Statistics.Services
{
    public class StatSummaryService : StatSummary.StatSummaryBase
    {
        private IStatSummaryIntegrator _summaryIntegrator;
        private IAtomicDictionary _atomicDictionary;
        private object _writeLock = new object();


        public StatSummaryService(IStatSummaryIntegrator summaryIntegrator, IAtomicDictionary atomicDictionary)
        {
            _summaryIntegrator = summaryIntegrator;
            _atomicDictionary = atomicDictionary;
        }


        public override Task<PostResponse> PostStatSummary(TwitterStatSummary data, ServerCallContext context)
        {
            lock (_writeLock)
            {
                var mergedData = _summaryIntegrator.IntegrateSummaryData(getData(), getSummaryModelFromMessage(data));
                saveData(mergedData);
            }

            return Task.FromResult<PostResponse>(new PostResponse { Success = true });
        }

        public override Task<TwitterStatSummary> GetLatestStats(NullMessage request, ServerCallContext context)
        {
            return Task.FromResult<TwitterStatSummary>(getGpcStatSummaryFromModel(getData()));
        }


        void saveData(m.TwitterStatSummary mergedData)
        {
            var changeSet = new List<Tuple<string, object>>();

            changeSet.Add(Tuple.Create<string, object>(Constants.DictionaryKeys.StartTimeKey, mergedData.FirstTweetTime));
            changeSet.Add(Tuple.Create<string, object>(Constants.DictionaryKeys.EndTimeKey, mergedData.LastTweetTime));
            changeSet.Add(Tuple.Create<string, object>(Constants.DictionaryKeys.CountAllTweetsKey, mergedData.NumberOfTweets));
            changeSet.Add(Tuple.Create<string, object>(Constants.DictionaryKeys.CountUrlTweetsKey, mergedData.NumberOfUrlTweets));
            changeSet.Add(Tuple.Create<string, object>(Constants.DictionaryKeys.CountPhotoUrlTweetsKey, mergedData.NumberOfPhotoUrlTweets));
            changeSet.Add(Tuple.Create<string, object>(Constants.DictionaryKeys.TopHashtagsKey, mergedData.HashtagCount));
            changeSet.Add(Tuple.Create<string, object>(Constants.DictionaryKeys.TopDomainsKey, mergedData.DomainCount));
            changeSet.Add(Tuple.Create<string, object>(Constants.DictionaryKeys.CountEmojiTweetsKey, mergedData.NumberEmojiTweets));
            changeSet.Add(Tuple.Create<string, object>(Constants.DictionaryKeys.TopEmojisKey, mergedData.EmojiCount));

            _atomicDictionary.Set(changeSet);
        }

        m.TwitterStatSummary getData()
        {
            var result = new m.TwitterStatSummary();

            result.FirstTweetTime = _atomicDictionary.Get<long>(Constants.DictionaryKeys.StartTimeKey);
            result.LastTweetTime = _atomicDictionary.Get<long>(Constants.DictionaryKeys.EndTimeKey);
            result.NumberOfTweets = _atomicDictionary.Get<int>(Constants.DictionaryKeys.CountAllTweetsKey);
            result.NumberOfUrlTweets = _atomicDictionary.Get<int>(Constants.DictionaryKeys.CountUrlTweetsKey);
            result.NumberOfPhotoUrlTweets = _atomicDictionary.Get<int>(Constants.DictionaryKeys.CountPhotoUrlTweetsKey);
            result.NumberEmojiTweets = _atomicDictionary.Get<int>(Constants.DictionaryKeys.CountEmojiTweetsKey);

            // dict's are null initially - if nothing has yet been stored. handle them a little more carefully...

            var dictValue = _atomicDictionary.Get<IDictionary<string, int>>(Constants.DictionaryKeys.TopDomainsKey);

            if (null != dictValue)
                result.DomainCount = dictValue;

            dictValue = _atomicDictionary.Get<IDictionary<string, int>>(Constants.DictionaryKeys.TopHashtagsKey);

            if(null != dictValue)
                result.HashtagCount = dictValue;

            dictValue = _atomicDictionary.Get<IDictionary<string, int>>(Constants.DictionaryKeys.TopEmojisKey);

            if (null != dictValue)
                result.EmojiCount = dictValue;

            return result;
        }

        m.TwitterStatSummary getSummaryModelFromMessage(TwitterStatSummary data)
        {
            return new m.TwitterStatSummary
            {
                FirstTweetTime = data.FirstTweetTime,
                LastTweetTime = data.LastTweetTime,
                NumberOfTweets = data.NumberOfTweets,
                NumberOfUrlTweets = data.NumberOfUrlTweets,
                NumberOfPhotoUrlTweets = data.NumberOfPhotoUrlTweets,
                HashtagCount = data.HashtagCount,
                DomainCount = data.DomainCount,
                NumberEmojiTweets = data.NumberEmojiTweets,
            };
        }
        
        TwitterStatSummary getGpcStatSummaryFromModel(m.TwitterStatSummary twitterStatSummary)
        {
            var result = new TwitterStatSummary
            {
                FirstTweetTime = twitterStatSummary.FirstTweetTime,
                LastTweetTime = twitterStatSummary.LastTweetTime,
                NumberOfTweets = twitterStatSummary.NumberOfTweets,
                NumberOfUrlTweets = twitterStatSummary.NumberOfUrlTweets,
                NumberOfPhotoUrlTweets = twitterStatSummary.NumberOfPhotoUrlTweets,
                NumberEmojiTweets = twitterStatSummary.NumberEmojiTweets,
            };

            twitterStatSummary.DomainCount.ToList().ForEach(kvp => result.DomainCount.Add(kvp.Key, kvp.Value));
            twitterStatSummary.HashtagCount.ToList().ForEach(kvp => result.HashtagCount.Add(kvp.Key, kvp.Value));

            return result;
        }

    }
}



//m.TwitterStatSummary getSummaryFromReadDictionary(HotSwappingReadWriteDictionary readWriteDictionary)
//{
//    return getSummaryFromConcurrentDictionary(readWriteDictionary.ReadTable);
//}

//m.TwitterStatSummary getSummaryFromWriteDictionary(HotSwappingReadWriteDictionary readWriteDictionary)
//{
//    return getSummaryFromConcurrentDictionary(readWriteDictionary.WriteTable);
//}

//m.TwitterStatSummary getSummaryFromConcurrentDictionary(ConcurrentDictionary<string, object> dictionary)
//{
//    var result = new m.TwitterStatSummary();

//    result.PeriodInMilliSeconds = _atomicDictionary.Get<long>(Constants.DictionaryKeys.PeriodInSecondsKey);
//    result.NumberOfTweets = _atomicDictionary.Get<int>(Constants.DictionaryKeys.CountAllTweetsKey);
//    result.NumberOfUrlTweets = _atomicDictionary.Get<int>(Constants.DictionaryKeys.CountUrlTweetsKey);
//    result.NumberOfPhotoUrlTweets = _atomicDictionary.Get<int>(Constants.DictionaryKeys.CountPhotoUrlTweetsKey);
//    result.HashtagCount = _atomicDictionary.Get<IDictionary<string, int>>(Constants.DictionaryKeys.TopHashtagsKey);
//    result.DomainCount = _atomicDictionary.Get<IDictionary<string, int>>(Constants.DictionaryKeys.TopDomainsKey);
//    result.NumberEmojiTweets = _atomicDictionary.Get<int>(Constants.DictionaryKeys.CountEmojiTweetsKey);
//    result.EmojiCount = _atomicDictionary.Get<IDictionary<string, int>>(Constants.DictionaryKeys.TopEmojisKey);

//    // object val;

//    //if (dictionary.TryGetValue(Constants.DictionaryKeys.PeriodInSecondsKey, out val))
//    //{
//    //    result.PeriodInMilliSeconds = (long)val;
//    //}

//    //val = null;

//    //if (dictionary.TryGetValue(Constants.DictionaryKeys.CountAllTweetsKey, out val))
//    //{
//    //    result.NumberOfTweets = (int)val;
//    //}

//    //val = null;

//    //if (dictionary.TryGetValue(Constants.DictionaryKeys.CountUrlTweetsKey, out val))
//    //{
//    //    result.NumberOfUrlTweets = (int)val;
//    //}

//    //val = null;

//    //if (dictionary.TryGetValue(Constants.DictionaryKeys.CountPhotoUrlTweetsKey, out val))
//    //{
//    //    result.NumberOfPhotoUrlTweets = (int)val;
//    //}

//    //val = null;

//    //if (dictionary.TryGetValue(Constants.DictionaryKeys.TopHashtagsKey, out val))
//    //{
//    //    result.HashtagCount = val as IDictionary<string, int>;
//    //}

//    //val = null;

//    //if (dictionary.TryGetValue(Constants.DictionaryKeys.TopDomainsKey, out val))
//    //{
//    //    result.DomainCount = val as IDictionary<string, int>;
//    //}

//    //val = null;

//    //if (dictionary.TryGetValue(Constants.DictionaryKeys.CountEmojiTweetsKey, out val))
//    //{
//    //    result.NumberEmojiTweets = (int)val;
//    //}

//    //val = null;

//    //if (dictionary.TryGetValue(Constants.DictionaryKeys.TopEmojisKey, out val))
//    //{
//    //    result.EmojiCount = val as Dictionary<string, int>;
//    //}

//    return result;
//}
