using JackHenry.Demo.Persistence.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace JackHenry.Demo.Libraries.ConcurrentAtomicDictionary
{
    public class ConcurrentAtomicDictionary : IAtomicDictionary
    {
        object _accessLock = new object();
        ConcurrentDictionary<string, object> _ht = new ConcurrentDictionary<string, object>();


        public T Get<T>(string key)
        {
            T result = default(T);
            object outValue;

            // need to lock for atomicity; if there's a set of values being persisted together we don't want just some of them, wait for all to finish
            lock (_accessLock)  
            {
                if (_ht.TryGetValue(key, out outValue))
                    result = (T)outValue;
            }

            return result;
        }

        public void Set(string key, object value)
        {
            lock (_accessLock)
            {
                _ht[key] = value;
            }
        }

        public void Set(IEnumerable<Tuple<string, object>> data)
        {
            if (null == data | 1 > data.Count())
                return;

            lock (_accessLock)
            {
                foreach (var datum in data)
                {
                    Set(datum.Item1, datum.Item2);
                }
            }
        }


    }
}
