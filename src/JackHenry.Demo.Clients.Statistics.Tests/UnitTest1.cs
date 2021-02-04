using NUnit.Framework;
using System.Threading.Tasks;

namespace JackHenry.Demo.Clients.Statistics.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task PostSummaryTest()
        {
            var testModel = new JackHenry.Demo.Libraries.Models.TwitterStatSummary
            { 
                NumberOfTweets= 100,
                NumberEmojiTweets = 20
            };
            var expectedReply = true;

            var client = new StatisticsServiceClient(getConfig());
            var reply = await client.PostStatisticSummaryAsync(testModel);

            Assert.AreEqual(expectedReply, reply);
        }

        StatisticsServiceClientConfiguration getConfig()
        {
            return new StatisticsServiceClientConfiguration
            {
                Host = "localhost",
                Port= 5000,
                UseSSL = false
            };
        }

    }
}