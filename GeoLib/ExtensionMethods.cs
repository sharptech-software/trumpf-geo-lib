using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fasteroid {

    internal class RegexFailException : Exception {
        public RegexFailException(string message) : base(message) { }
    }

    internal static class ExtensionMethods {
        public static V GetOrAdd<K, V>(this Dictionary<K, V> self, K key) where K : notnull where V : new() {
            if (!self.TryGetValue(key, out V? value) ) {
                value = new V();
                self[key] = value;
            }
            return value;
        }

        public static V GetOrElse<K, V>(this Dictionary<K, V> self, K key, string assertion) where K : notnull {
            if(!self.TryGetValue(key, out V? value) ) {
                throw new KeyNotFoundException(assertion);
            }
            return value;
        }

        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> self, K key, V defaultValue) where K : notnull {
            if (!self.TryGetValue(key, out V? value) ) {
                return defaultValue;
            }
            return value;
        }

        public static Match MatchOrElse(this Regex self, string input, string message) {
            var match = self.Match(input);
            if (!match.Success) {
                throw new RegexFailException(message);
            }
            return match;
        }

        public static ReadOnlySpan<char> TakeInt(this ReadOnlySpan<char> self, out int value) {
            var ret = self.TakeLines(1, out string str_int);
            value = int.Parse(str_int);
            return ret;
        }

    }

}
