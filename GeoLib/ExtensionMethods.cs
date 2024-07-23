using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fasteroid {
    internal static class ExtensionMethods {
        public static V GetOrAdd<K, V>(this Dictionary<K, V> dict, K key) where K : notnull where V : new() {
            if (!dict.ContainsKey(key)) {
                dict[key] = new V();
            }
            return dict[key];
        }
    }
}
