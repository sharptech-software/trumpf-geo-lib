using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {

        [GeneratedRegex(@"^#~(\d+)(.*?)#?#~", RegexOptions.Singleline | RegexOptions.Multiline)]
        private static partial Regex SectionPattern();

        [GeneratedRegex(@"(?>\n)?(.*?)(?>\n)\|~", RegexOptions.Singleline | RegexOptions.Multiline)]
        private static partial Regex BlockPattern();

        internal static async Task< Dictionary<int, List<List<string>>> > Load(string path) {

            // section type -> list of sections -> list of blocks
            var geo = new Dictionary<int, List<List<string>>>();
            
            string data = ( await File.ReadAllTextAsync(path) ).Replace("\r\n", "\n");

            var sectionMatches = SectionPattern().Matches(data);

            foreach (Match sectionMatch in sectionMatches) {
                var sectionList = geo.GetOrAdd( int.Parse( sectionMatch.Groups[1].Value ) );
                
                var blockMatches = BlockPattern().Matches(sectionMatch.Groups[2].Value);
                var blocks = new List<string>();

                if (blockMatches.Count == 0) {
                    blocks.Add( sectionMatch.Groups[2].Value );
                    continue;
                }

                foreach (Match blockMatch in blockMatches) {
                    blocks.Add( blockMatch.Groups[1].Value );
                }

                sectionList.Add(blocks);
            }

            return geo;

        }

    }
}
