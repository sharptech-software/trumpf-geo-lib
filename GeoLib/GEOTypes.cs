using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fasteroid {
    public partial class GEOLib {
        public static class TYPES {

            public static class SECTION {
                public const int HEADER   = 1;
                public const int POINTS   = 31;
                public const int TEXT     = 32;
                public const int ENTITIES = 331;
            }

            public static class ENTITY {
                public const string LINE   = "LIN";
                public const string ARC    = "ARC";
                public const string CIRCLE = "CIR";
            }

            // there are probably 8 colors, for all states representable with 3 bits, judging by what colors actually show up when you view a GEO:
            public static class COLORS { 
                public const int BLACK   = 1;
                // red?
                public const int YELLOW  = 3;
                public const int GREEN   = 4;
                // cyan?
                // blue?
                // magenta?
                // white?
            }

            // this is really uncertain at the moment:
            public static class STROKES {
                public const int DASHED = 0; // todo: verify
                public const int SOLID  = 1; // todo: verify
            }
        }
    }
}
