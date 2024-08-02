using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fasteroid {
    public partial class GEOLib {
        public class Attribute {
            public int Type { get; }

            public Attribute(int type) {
                Type = type;
            }

            public static (int, Attribute) FromBlock(string block) {
                var attdata = block.SkipLines(1) // "ATT"
                                   .TakeLines(1, out string strid)
                                   .TakeLines(1, out string strtype);
                return (
                    int.Parse(strid),
                    new Attribute( int.Parse(strtype) )
                );
            }
        }
    }
}
