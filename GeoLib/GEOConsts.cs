using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fasteroid {
    public partial class GEOLib {

        /// <summary>
        /// RE - Regular Expressions
        /// </summary>
        public static class RE {
            public const string DEC = @"-?\d+(?>\.\d+)?";
            public const string INT = @"-?\d+";
        }

        public static class TYPES {

            public static class SECTION {
                public const int HEADER   = 1;
                public const int POINTS   = 31;
                public const int TEXT     = 32;
                public const int ATT      = 36;
                public const int ENTITIES = 331;
            }

            public static class ATT {
                public const int TEXT_SLAVE = 10;
            }

            public static class ENTITY {
                public const string LINE   = "LIN";
                public const string ARC    = "ARC";
                public const string CIRCLE = "CIR";
            }

            public static class COLORS { 
                public const int DEFAULT = 1;
                public const int RED     = 2;
                public const int YELLOW  = 3;
                public const int GREEN   = 4;
                public const int CYAN    = 5;
                public const int BLUE    = 6;
                public const int MAGENTA = 7;
            }

            public static class STROKES {
                public const int SOLID    = 0;
                public const int DASH     = 1;
                public const int DOT      = 2;
                public const int DASHDOT  = 3;
            }
        }
    }
}
