using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fasteroid.GEOLib {

    public class GEORawEntity {

    }

    public class GEORawBlock {
        
    }


    public partial class GEOLoader {


        [GeneratedRegex(@"^#~(\d+)(.*?)#?#~", RegexOptions.Singleline | RegexOptions.Multiline)]
        private static partial Regex SectionPattern();

        [GeneratedRegex(@"(?>\n)?(.*?)(?>\n)\|~", RegexOptions.Singleline | RegexOptions.Multiline)]
        private static partial Regex EntityPattern();

        public static async Task< Dictionary<string, List<string[]>> > Load(string path) {

            // block type -> list of blocks of that type -> each is a list of lines
            var geo = new Dictionary<string, List<string[]>>();
            
            string data = ( await File.ReadAllTextAsync(path) ).Replace("\r\n", "\n");

            var sectionMatches = SectionPattern().Matches(data);

            foreach (Match sectionMatch in sectionMatches) {
                var section = geo.GetOrAdd( sectionMatch.Groups[1].Value );
                
                var entityMatches = EntityPattern().Matches(sectionMatch.Groups[2].Value);

                if (entityMatches.Count == 0) {
                    section.Add( sectionMatch.Groups[2].Value.Split('\n') );
                    continue;
                }

                foreach (Match entityMatch in entityMatches) {
                    var entity = entityMatch.Groups[1].Value.Split('\n');
                    section.Add(entity);
                }
            }

            return geo;

        }

    }
}
