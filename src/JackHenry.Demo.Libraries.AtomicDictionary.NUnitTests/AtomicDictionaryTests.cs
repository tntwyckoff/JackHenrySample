using JackHenry.Demo.Libraries.ConcurrentAtomicDictionary;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JackHenry.Demo.Libraries.HotSwap.NUnitTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ConstructorCreatesBothChannels()
        {
            var rw = new HotSwappingReadWriteDictionary();

            Assert.NotNull(rw.ReadTable);
            Assert.NotNull(rw.WriteTable);
            Assert.AreNotEqual(rw.ReadObjectId, rw.WriteObjectId);
        }

        [Test]
        public async Task EmptyWriteDictionaryDoesntSwap()
        {
            var rw = new HotSwappingReadWriteDictionary(new HotSwapOptions { SwapIntervalMilliseconds = 500 }); ;
            var readId = rw.ReadObjectId;
            var writeId = rw.WriteObjectId;


            await Task.Delay(750); // allows for a potential swap, which shouldn't happen

            Assert.AreEqual(readId, rw.ReadObjectId);
            Assert.AreEqual(writeId, rw.WriteObjectId);
        }

        [Test]
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
            Thread.Sleep(600);

            // await Task.Delay(500);  // ensures there's at least one swap opportunity

            Assert.AreEqual(readId, rw.WriteObjectId);
            Assert.AreEqual(writeId, rw.ReadObjectId);
        }

        [Test]
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

            Assert.AreEqual(readId, rw.ReadObjectId);
            Assert.AreEqual(writeId, rw.WriteObjectId);

            Console.WriteLine($"After second sleep: readId: {rw.ReadObjectId}, writeId: {rw.WriteObjectId}");

            for (int index = 10; index < 20; index++)
            {
                rw.Set(index.ToString(), $"This is item {index + 1}");
            }

            Thread.Sleep(500);

            Console.WriteLine($"After second sleep: readId: {rw.ReadObjectId}, writeId: {rw.WriteObjectId}");

            // swap should have occurred now, ids should be reversed

            Assert.AreEqual(readId, rw.WriteObjectId);
            Assert.AreEqual(writeId, rw.ReadObjectId);

            Thread.Sleep(500);

            Console.WriteLine($"After second sleep: readId: {rw.ReadObjectId}, writeId: {rw.WriteObjectId}");

            // no swap should have occurred, ids should the same as prevs

            Assert.AreEqual(readId, rw.WriteObjectId);
            Assert.AreEqual(writeId, rw.ReadObjectId);
        }

    }
}