using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackHenry.Demo.Clients.Statistics
{
    public class StatisticsServiceClientConfiguration
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5000;
        public bool UseSSL { get; set; } = false;
        internal string SslProtocolOrNull { get { return UseSSL ? "s" : ""; } }
        public string ServiceAddress { get { return $"http{SslProtocolOrNull}://{Host}:{Port}"; } }
    }
}
