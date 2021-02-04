using JackHenry.Demo.Libraries.ConcurrentAtomicDictionary;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace JackHenry.Demo.Libraries.HotSwap.Tests
{
    public class HotSwapDictionaryTests
    {
        private ITestOutputHelper _output;


        public HotSwapDictionaryTests(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void ConstructorCreatesBothChannels()
        {
            var rw = new HotSwappingReadWriteDictionary();

            Assert.NotNull(rw.ReadTable);
            Assert.NotNull(rw.WriteTable);
            Assert.NotEqual<int>(rw.ReadObjectId, rw.WriteObjectId);
        }

        [Fact]
        public async Task EmptyWriteDictionaryDoesntSwap()
        {
            var rw = new HotSwappingReadWriteDictionary(new HotSwapOptions { SwapIntervalMilliseconds = 500 }); ;
            var readId = rw.ReadObjectId;
            var writeId = rw.WriteObjectId;


            await Task.Delay(750); // allows for a potential swap, which shouldn't happen

            Assert.Equal<int>(readId, rw.ReadObjectId);
            Assert.Equal<int>(writeId, rw.WriteObjectId);
        }

        [Fact]
        public void WriteDictionaryGetsSwapped()
        {
            var rw = new HotSwappingReadWriteDictionary(new HotSwapOptions { SwapIntervalMilliseconds = 500 }); ;
            var readId = rw.ReadObjectId;
            var writeId = rw.WriteObjectId;

            for (int index = 0; index < 10; index++)
            {
                rw.Set(index.ToString(), $"This is item {index + 1}");
            }

            rw.Stop();              // doesn't stop immediately; only after an interval
            Thread.Sleep(700);

            // await Task.Delay(500);  // ensures there's at least one swap opportunity

            Assert.Equal<int>(readId, rw.WriteObjectId);
            Assert.Equal<int>(writeId, rw.ReadObjectId);
        }

        [Fact]
        public void LongerRunningTest()
        {
            var rw = new HotSwappingReadWriteDictionary(new HotSwapOptions { SwapIntervalMilliseconds = 500 }); ;

            for (int index = 0; index < 10; index++)
            {
                rw.Set(index.ToString(), $"This is item {index + 1}");
            }

            Thread.Sleep(600);

            var readId = rw.ReadObjectId;
            var writeId = rw.WriteObjectId;

            // allow another interval to happen without adding any data; swapping should not occur

            Thread.Sleep(500);

            Assert.Equal<int>(readId, rw.ReadObjectId);
            Assert.Equal<int>(writeId, rw.WriteObjectId);

            _output.WriteLine($"After second sleep: readId: {rw.ReadObjectId}, writeId: {rw.WriteObjectId}");

            for (int index = 10; index < 20; index++)
            {
                rw.Set(index.ToString(), $"This is item {index + 1}");
            }

            Thread.Sleep(500);

            _output.WriteLine($"After second sleep: readId: {rw.ReadObjectId}, writeId: {rw.WriteObjectId}");

            // swap should have occurred now, ids should be reversed

            Assert.Equal<int>(readId, rw.WriteObjectId);
            Assert.Equal<int>(writeId, rw.ReadObjectId);

            Thread.Sleep(500);

            _output.WriteLine($"After second sleep: readId: {rw.ReadObjectId}, writeId: {rw.WriteObjectId}");

            // no swap should have occurred, ids should the same as prevs

            Assert.Equal<int>(readId, rw.WriteObjectId);
            Assert.Equal<int>(writeId, rw.ReadObjectId);
        }

    }
}
