using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackHenry.Demo.Clients.TwitterApi
{
    public class TwitterClientConfiguration
    {
        public string BearerToken { get; set; }
        public int ConnectionRetryAttempts { get; set; } = -1;          // -1 == retry forever
        public int BatchSize { get; set; } = 50;
    }
}
