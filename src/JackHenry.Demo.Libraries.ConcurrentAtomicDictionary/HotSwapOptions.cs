using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackHenry.Demo.Libraries.ConcurrentAtomicDictionary
{
    public class HotSwapOptions
    {
        public int SwapIntervalMilliseconds { get; set; } = 2000;

        public static HotSwapOptions DefaultOptions = new HotSwapOptions();

    }
}
