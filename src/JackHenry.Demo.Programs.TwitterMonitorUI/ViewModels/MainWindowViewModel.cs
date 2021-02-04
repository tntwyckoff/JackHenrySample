using JackHenry.Demo.Clients.Statistics;
using JackHenry.Demo.Libraries.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JackHenry.Demo.Programs.TwitterMonitorUI.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private StatisticsServiceClient _statsClient;
        private bool _loading = true;
        private bool _ready = false;
        private long _totalTweets;
        private long _tweetsWithUrls;
        private long _tweetsWithPhotoUrls;
        private long _elapsedTimeInMs;
        private Timer _refreshTimer;


        public string Title { get; set; } = "Twitter Monitor";

        public bool Loading
        {
            get { return _loading; }
            set 
            {
                if (_loading == value)
                    return;

                SetProperty(ref _loading, value);
                Ready = !_loading;
            }
        }

        public bool Ready
        {
            get { return _ready; }
            set { SetProperty(ref _ready, value); }
        }

        public long TotalTweets
        {
            get { return _totalTweets; }
            set { SetProperty(ref _totalTweets, value); }
        }

        public long ElapsedTimeInMs
        {
            get { return _elapsedTimeInMs; }
            set { SetProperty(ref _elapsedTimeInMs, value); }
        }

        public long ElapsedTimeInSeconds
        {
            get { return _elapsedTimeInMs / 1000; }
        }

        public string ElapsedTimeText
        {
            get
            {
                if (1 > ElapsedTimeInMs)
                    return "-";

                TimeSpan ts = TimeSpan.FromMilliseconds(ElapsedTimeInMs);

                if (1 > ts.Minutes)
                    return $"{ts.Seconds} seconds";

                if (1 > ts.Hours)
                    return $"{ts.Minutes} minute(s), {ts.Seconds} seconds";

                return $"{ts.Hours} hour(s), {ts.Minutes} minute(s), {ts.Seconds} seconds";
            }
        }

        public double TweetsPerSecond
        {
            get
            {
                if (1 > ElapsedTimeInMs)
                    return 0d;

                return (double)TotalTweets / ElapsedTimeInSeconds;
            }
        }

        public double TweetsPerMinute
        {
            get
            {
                if (1 > ElapsedTimeInMs)
                    return 0d;

                return TweetsPerSecond * 60d;
            }
        }

        public double TweetsPerHour
        {
            get
            {
                if (1 > ElapsedTimeInMs)
                    return 0d;

                return TweetsPerMinute * 60d;
            }
        }

        public long TweetsWithUrls
        {
            get { return _tweetsWithUrls; }
            set { SetProperty(ref _tweetsWithUrls, value); }
        }

        public double PctWithUrls
        {
            get
            {
                if (1 > TotalTweets)
                    return 0d;

                return (double)TweetsWithUrls / (double)TotalTweets;
            }
        }

        public long TweetsWithPhotoUrls
        {
            get { return _tweetsWithPhotoUrls; }
            set { SetProperty(ref _tweetsWithPhotoUrls, value); }
        }

        public double PctWithPhotoUrls
        {
            get
            {
                if (1 > TotalTweets)
                    return 0d;

                return (double)TweetsWithPhotoUrls / (double)TotalTweets;
            }
        }

        public List<Tuple<string, int>> TopDomains { get; private set; }

        public List<Tuple<string, int>> TopHashtags { get; private set; }


        public MainWindowViewModel(StatisticsServiceClient statsClient)
        {
            _statsClient = statsClient;
            startStatMonitoring();
        }


        void startStatMonitoring()
        {
            _refreshTimer = new Timer(refreshData, null, 1000, 1000);
        }

        async void refreshData(object state)
        {
            await getLatestSummary().ContinueWith(updateStats);
        }

        Task<TwitterStatSummary> getLatestSummary()
        {
            return _statsClient.GetLatestStats();
        }

        void updateStats(Task<TwitterStatSummary> source)
        {
            Loading = false;
            TotalTweets = source.Result.NumberOfTweets;
            ElapsedTimeInMs = (long)TimeSpan.FromTicks(source.Result.LastTweetTime - source.Result.FirstTweetTime).TotalMilliseconds;
            TweetsWithUrls = source.Result.NumberOfUrlTweets;
            TweetsWithPhotoUrls = source.Result.NumberOfPhotoUrlTweets;
            TopDomains = source.Result.DomainCount.OrderByDescending(k => k.Value).Take(6).Select(k => Tuple.Create<string, int>(k.Key, k.Value)).ToList();
            TopHashtags = source.Result.HashtagCount.OrderByDescending(k => k.Value).Take(6).Select(k => Tuple.Create<string, int>(k.Key, k.Value)).ToList();

            RaisePropertyChanged("ElapsedTimeText");
            RaisePropertyChanged("TweetsPerSecond");
            RaisePropertyChanged("TweetsPerMinute");
            RaisePropertyChanged("TweetsPerHour");
            RaisePropertyChanged("TweetsWithUrls");
            RaisePropertyChanged("PctWithUrls");
            RaisePropertyChanged("TweetsWithPhotoUrls");
            RaisePropertyChanged("PctWithPhotoUrls");
            RaisePropertyChanged("TopDomains");
            RaisePropertyChanged("TopHashtags");
        }

    }
}
