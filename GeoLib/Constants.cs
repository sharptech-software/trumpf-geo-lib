namespace Fasteroid {
    public partial class GEOLib {

        /// <summary>
        /// RE - Regular Expressions
        /// </summary>
        public static class RE {
            public const string DEC = @"-?\d+(?>\.\d+)?";
            public const string INT = @"-?\d+";
        }

        public static class CONSTANTS {

            public static class SECTION {
                public const int HEADER        = 1;
                public const int POINTS        = 31;
                public const int TEXT          = 32;
                public const int ATT           = 36;
                public const int ENTITIES      = 331;
                public const int BEND_ENTITIES = 371;
            }


            public static class ATTRIBUTE {
                public const int TEXT_SLAVE   = 10;
                public const int PROCESS_TYPE = 18; // does nothing right now but nice to keep track of
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

                public static string Lookup(int color) {
                    return color switch {
                        RED     => "#f00",
                        YELLOW  => "#ff0",
                        GREEN   => "#0f0",
                        CYAN    => "#0ff",
                        BLUE    => "#00f",
                        MAGENTA => "#f0f",
                        _       => "#000"
                    };
                }
            }

            public static class STROKES {
                public const int SOLID    = 0;
                public const int DASH     = 1;
                public const int DOT      = 2;
                public const int DASHDOT  = 3;

                public static string? Lookup(int stroke) {
                    return stroke switch {
                        DASH    => "5,5",
                        DOT     => "1,5",
                        DASHDOT => "5,5,1,5",
                        _       => null
                    };
                }
            }
        }
    }
}
