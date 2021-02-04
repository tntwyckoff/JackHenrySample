using JackHenry.Demo.Clients.TwitterApi.ApiModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JackHenry.Demo.Clients.TwitterApi
{
    public class TwitterSampleStreamPump
    {
        private TwitterClientConfiguration _config;
        private ILogger<TwitterSampleStreamPump> _logger;
        private HttpClient _httpClient;
        private Lazy<ConcurrentQueue<Tweet>> _lBatchedTweets = new Lazy<ConcurrentQueue<Tweet>>();
        private Action<List<Tweet>> _clientCallback;
        private CancellationToken _cancelToken;
        private int _batchSize = 20;
        private int _actualRetries = 0;

        string SourceUri { get { return $"{Constants.Twitter.SampledStreamEp}?{Constants.Twitter.FieldsParam}=attachments,author_id,created_at,entities"; } }

        HttpClient HttpClient 
        {
            get 
            {
                if (null == _httpClient)
                    _httpClient = buildHttpClient();

                return _httpClient;
            }
        }


        public TwitterSampleStreamPump(TwitterClientConfiguration config, ILogger<TwitterSampleStreamPump> logger)
        {
            _config = config;
            _logger = logger;

            if (0 < _config.BatchSize)
                _batchSize = _config.BatchSize;
        }


        public async Task DownloadSampleStreamChunked(Action<List<Tweet>> callback, CancellationToken cancelToken, int? batchSize = null)
        {
            _clientCallback = callback;
            _cancelToken = cancelToken;

            if (batchSize.HasValue)
                _batchSize = batchSize.Value;

            startPump(cancelToken);

            while (0 > _config.ConnectionRetryAttempts || _actualRetries <= _config.ConnectionRetryAttempts)
            {
                if (0 < _actualRetries)
                {
                    _logger.LogWarning($"Unable to connect to Twitter API. Re-try attempt {_actualRetries}");
                    Thread.Sleep(1000 * _actualRetries);
                }

                try
                {
                    using (var response = await HttpClient.GetAsync(SourceUri, HttpCompletionOption.ResponseHeadersRead))
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var reader = new StreamReader(stream))
                            {
                                while (!reader.EndOfStream)
                                {
                                    if (_cancelToken.IsCancellationRequested)
                                        break;

                                    _actualRetries = 0; // if we had issues but we've recovered, reset

                                    var line = reader.ReadLine();

                                    if (String.IsNullOrEmpty(line))
                                        continue;

                                    try
                                    {
                                        var datum = JsonSerializer.Deserialize<Datum>(line);

                                        _logger.LogDebug($"{datum.data.text}");

                                        _lBatchedTweets.Value.Enqueue(datum.data);
                                    }
                                    catch
                                    {
                                        // this try/catch is to handle unforeseen, one-off, funky formatting/content issues that can't be deserialized
                                        // if this happens just log it/ignore it; we missed one and we'll keep receiving
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // if here it's network/stream/buffer related
                    ++_actualRetries;
                }
            }

            _logger.LogInformation("DownloadSampleStreamChunked exiting.");
        }


        private void startPump(CancellationToken cancelToken)
        {
            Task.Run(async () =>
            {
                do
                {
                    if (cancelToken.IsCancellationRequested)
                        break;

                    if (_batchSize <= _lBatchedTweets.Value.Count)
                    {
                        _logger.LogInformation($"Batch size of {_batchSize} tweets reached, clearing cache...");
                        _ = Task.Run(() => internalPushBatch());
                    }

                    Thread.Sleep(250);
                } while (true);
            });
        }

        private void internalPushBatch()
        {
            if (null == _clientCallback)
                return;

            var localList = new List<Tweet>();

            for (int index = 0; index < _batchSize; index++)
            {
                Tweet t;

                if(_lBatchedTweets.Value.TryDequeue(out t))
                    localList.Add(t);
            }

            var reamining = _lBatchedTweets.Value.Count;

            _clientCallback(localList.ToList());

            _logger.LogInformation($"{_batchSize} items cleared: {reamining} tweets remaining.");
        }

        private HttpClient buildHttpClient()
        {
            var result = new HttpClient();

            result.BaseAddress = new Uri(Constants.Twitter.ApiBase);
            result.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.BearerToken);
            result.Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite);

            return result;
        }

    }
}
