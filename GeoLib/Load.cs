using System.Text.RegularExpressions;

namespace SharpTech {
    public partial class GEOLib {

        [GeneratedRegex(@"^#~(\d+)(.*?)#?#~"  , RegexOptions.Singleline | RegexOptions.Multiline)]
        private static partial Regex SectionPattern();

        [GeneratedRegex(@"(?>\n)?(.*?)(?>\n)\|~"  , RegexOptions.Singleline | RegexOptions.Multiline)]
        private static partial Regex BlockPattern();

        internal static async Task< Dictionary<int, List<string>> > Load(string path) {

            // block type -> section of blocks -> each block in the section is a string
            var geo = new Dictionary<int, List<string>>();
            
            string data = (await File.ReadAllTextAsync(path)).Replace("\r\n", "\n");

            var sectionMatches = SectionPattern().Matches(data);

            foreach (Match sectionMatch in sectionMatches) {
                var section = geo.GetOrAdd(int.Parse(sectionMatch.Groups[1].Value));
                
                var blockMatches = BlockPattern().Matches(sectionMatch.Groups[2].Value);

                if (blockMatches.Count == 0) {
                    section.Add(sectionMatch.Groups[2].Value);
                    continue;
                }

                foreach (Match blockMatch in blockMatches) {
                    section.Add(blockMatch.Groups[1].Value);
                }
            }

            return geo;

        }

    }
}
