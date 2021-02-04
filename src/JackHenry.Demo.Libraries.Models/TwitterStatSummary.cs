using System;
using System.Collections.Generic;

namespace JackHenry.Demo.Libraries.Models
{
    public class TwitterStatSummary
    {
        public long FirstTweetTime { get; set; }
        public long LastTweetTime { get; set; }
        public int NumberOfTweets { get; set; }
        public int NumberOfUrlTweets { get; set; }
        public int NumberOfPhotoUrlTweets { get; set; }
        public int NumberEmojiTweets { get; set; }
        public IDictionary<string, int> HashtagCount { get; set; } = new Dictionary<string, int>();
        public IDictionary<string, int> DomainCount { get; set; } = new Dictionary<string, int>();
        public IDictionary<string, int> EmojiCount { get; set; } = new Dictionary<string, int>();
    }
}
