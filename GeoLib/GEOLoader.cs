using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {

        [GeneratedRegex(@"^#~(\d+)(.*?)#?#~", RegexOptions.Singleline | RegexOptions.Multiline)]
        private static partial Regex SectionPattern();

        [GeneratedRegex(@"(?>\n)?(.*?)(?>\n)\|~", RegexOptions.Singleline | RegexOptions.Multiline)]
        private static partial Regex EntityPattern();

        internal static async Task< Dictionary<int, List<string>> > Load(string path) {

            // block type -> list of blocks of that type -> each is a list of lines
            var geo = new Dictionary<int, List<string>>();
            
            string data = ( await File.ReadAllTextAsync(path) ).Replace("\r\n", "\n");

            var sectionMatches = SectionPattern().Matches(data);

            foreach (Match sectionMatch in sectionMatches) {
                var section = geo.GetOrAdd( int.Parse( sectionMatch.Groups[1].Value ) );
                
                var entityMatches = EntityPattern().Matches(sectionMatch.Groups[2].Value);

                if (entityMatches.Count == 0) {
                    section.Add( sectionMatch.Groups[2].Value );
                    continue;
                }

                foreach (Match entityMatch in entityMatches) {
                    section.Add( entityMatch.Groups[1].Value );
                }
            }

            return geo;

        }

    }
}
