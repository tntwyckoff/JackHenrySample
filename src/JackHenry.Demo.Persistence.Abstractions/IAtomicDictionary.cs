using System;
using System.Collections.Generic;

namespace JackHenry.Demo.Persistence.Abstractions
{
    public interface IAtomicDictionary
    {
        T Get<T>(string key);
        void Set(string key, object value);
        void Set(IEnumerable<Tuple<string, object>> data);
    }
}
