using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fasteroid {
    public partial class GEOLib {
        public static class TYPES {
            public enum SECTION {
                HEADER   = 1,
                POINTS   = 31,
                TEXT     = 32,
                ENTITIES = 331,
            }
            public static class ENTITY {
                public static readonly string POINT  = "P";
                public static readonly string LINE   = "LIN";
                public static readonly string TEXT   = "TXT";
                public static readonly string ARC    = "ARC";
                public static readonly string CIRCLE = "CIR";
            }
        }
    }
}
