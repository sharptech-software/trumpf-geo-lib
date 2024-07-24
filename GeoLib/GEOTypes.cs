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
                public const string TEXT   = "TXT";
                public const string ARC    = "ARC";
                public const string CIRCLE = "CIR";
            }
        }
    }
}
