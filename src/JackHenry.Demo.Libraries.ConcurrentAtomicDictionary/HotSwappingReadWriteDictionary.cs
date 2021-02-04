using JackHenry.Demo.Persistence.Abstractions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JackHenry.Demo.Libraries.ConcurrentAtomicDictionary
{
    public class HotSwappingReadWriteDictionary : IAtomicDictionary
    {
        object _swapLock = new object();
        ConcurrentDictionary<string, object> _ht1 = new ConcurrentDictionary<string, object>();
        ConcurrentDictionary<string, object> _ht2 = new ConcurrentDictionary<string, object>();
        byte _read = 1;
        bool _swapping = true;
        bool _dirty = false;


        public ConcurrentDictionary<string, object> ReadTable 
        { 
            get 
            {
                lock (_swapLock)
                {
                    return 1 == _read ? _ht1 : _ht2;
                }
            } 
        }

        public ConcurrentDictionary<string, object> WriteTable 
        { 
            get 
            {
                lock (_swapLock)
                {
                    return 1 == _read ? _ht2 : _ht1;
                }
            } 
        }

        public int ReadObjectId { get { lock (_swapLock) { return ReadTable.GetHashCode(); } } }

        public int WriteObjectId { get { lock (_swapLock) { return WriteTable.GetHashCode(); } } }


        public HotSwappingReadWriteDictionary(HotSwapOptions options = null)
        {
            if (null == options)
                options = HotSwapOptions.DefaultOptions;

            startSwapping(options.SwapIntervalMilliseconds);
        }


        public void Set(string key, object value)
        {
            lock (_swapLock)
            {
                WriteTable[key] = value;
                _dirty = true;
            }
        }

        public void Set(IEnumerable<Tuple<string, object>> data)
        {
            if (null == data | 1 > data.Count())
                return;

            lock (_swapLock)
            {
                foreach (var datum in data)
                {
                    Set(datum.Item1, datum.Item2);
                }
            }
        }

        public void Stop()
        {
            lock (_swapLock)
            {
                if (!_swapping)
                    return;

                _swapping = false;
            }
        }

        public T Get<T>(string key)
        {
            T result = default(T);
            object outValue;

            if (ReadTable.TryGetValue(key, out outValue))
                result = (T)outValue;

            return result;
        }


        void startSwapping(int interval)
        {
            Action bgSwapCode = () =>
            {
                bool _localSwapping;

                lock (_swapLock)
                {
                    _localSwapping = _swapping;
                }

                while (_localSwapping)
                {
                    Thread.Sleep(interval);

                    lock (_swapLock)
                    {
                        // do the swap, even if stop command was issued b/c we just resumed while some items may have been added. 
                        //  otherwise we may lose items- they could have been written and not copied out to the read channel

                        swap();
                        _localSwapping = _swapping;     // this breaks the loop if stop was called
                    }
                }

                // out of the loop, this thread should just exit
                var o = new object();
            };

            Task.Run(bgSwapCode);
        }

        void swap()
        {
            lock (_swapLock)
            {
                if (WriteTable.IsEmpty || !_dirty)
                    return;

                _read = 1 == _read ? 2 : 1;

                WriteTable.Clear();

                var keys = ReadTable.Keys.ToList();

                foreach (var key in keys)
                {
                    WriteTable.TryAdd(key, ReadTable[key]);
                }

                _dirty = false;
            }
        }

    }
}