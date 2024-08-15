using System.Text.RegularExpressions;

namespace SharpTech {
    public partial class GEOLib {

        /// <summary>
        /// A 2D point class with a few vector utility operations.
        /// </summary>
        public partial class Point(double x, double y) {

            /// <summary>
            /// Equivalent to (0, 0)
            /// </summary>
            public readonly static Point ZERO = new Point(0,0);

            [GeneratedRegex($@"({RE.INT})\r?\n({RE.DEC}) ({RE.DEC}) {RE.DEC}"  , RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public readonly double X = x;
            public readonly double Y = y;

            public static Point operator +(Point a, Point b) {
                return new Point(a.X + b.X, a.Y + b.Y);
            }

            public static Point operator -(Point a, Point b) {
                return new Point(a.X - b.X, a.Y - b.Y);
            }

            /// <summary>
            /// Distance between two points.
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public double Distance(Point other) {
                return Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2));
            }

            /// <summary>
            /// Magnitude of this point.
            /// </summary>
            /// <returns></returns>
            public double Length() {
                return ZERO.Distance(this); // lol
            }

            internal static (int, Point) FromBlock(string block) {
                var match = Pattern().MatchOrElse(block, $"Malformed point: {block}"  );

                return (
                    int.Parse(match.Groups[1].Value),
                    new Point(
                        double.Parse(match.Groups[2].Value),
                        -double.Parse(match.Groups[3].Value) // invert y to match SVG coordinate system
                    )
                );
            }

        }
    }
}
