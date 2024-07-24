using System;
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
            if (!self.ContainsKey(key)) {
                self[key] = new V();
            }
            return self[key];
        }

        public static V GetOrElse<K, V>(this Dictionary<K, V> self, K key, string assertion) where K : notnull {
            if(!self.ContainsKey(key)) {
                throw new KeyNotFoundException(assertion);
            }
            return self[key];
        }

        public static V GetValueOrDefault<K, V>(this Dictionary<K, V> self, K key, V defaultValue) where K : notnull {
            if (!self.ContainsKey(key)) {
                return defaultValue;
            }
            return self[key];
        }

        public static Match MatchOrElse(this Regex self, string input, string message) {
            var match = self.Match(input);
            if (!match.Success) {
                throw new RegexFailException(message);
            }
            return match;
        }
    }
}
