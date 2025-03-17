using System.Text.RegularExpressions;

namespace SharpTech {

    /// <summary>
    /// Library for parsing TRUMPF's proprietary GEO file format.
    /// </summary>
    public partial class GEOLib {

        [GeneratedRegex(@"^#~(\d+)(.*?)#?#~"  , RegexOptions.Singleline | RegexOptions.Multiline)]
        private static partial Regex SectionPattern();

        [GeneratedRegex(@"(?>\n)?(.*?)(?>\n)\|~"  , RegexOptions.Singleline | RegexOptions.Multiline)]
        private static partial Regex BlockPattern();

        internal static Dictionary<int, List< List<string> >> Load(string fileContentsString)
        {
            // section type -> list of all times it was declared -> each block in each group is a string
            var geo = new Dictionary<int, List< List<string> >>();

            string data = (fileContentsString).Replace("\r\n", "\n");

            var sectionMatches = SectionPattern().Matches(data);

            foreach (Match sectionMatch in sectionMatches)
            {
                var section = geo.GetOrAdd(int.Parse(sectionMatch.Groups[1].Value));

                var blockMatches = BlockPattern().Matches(sectionMatch.Groups[2].Value);

                var group = new List<string>();

                if (blockMatches.Count == 0)
                {
                    group.Add(sectionMatch.Groups[2].Value);
                }
                else foreach (Match blockMatch in blockMatches)
                {
                    group.Add(blockMatch.Groups[1].Value);
                }

                section.Add(group);
            }

            return geo;
        }

    }
}
