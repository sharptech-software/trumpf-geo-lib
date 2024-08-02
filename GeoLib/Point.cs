using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fasteroid {
    public partial class GEOLib {

        public partial class Point {

            public readonly static Point ZERO = new Point(0,0);

            [GeneratedRegex($@"({RE.INT})\r?\n({RE.DEC}) ({RE.DEC}) {RE.DEC}", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public readonly float X;
            public readonly float Y;

            public Point(float x, float y) {
                X = x;
                Y = y;
            }

            public static Point operator +(Point a, Point b) {
                return new Point(a.X + b.X, a.Y + b.Y);
            }

            public static Point operator -(Point a, Point b) {
                return new Point(a.X - b.X, a.Y - b.Y);
            }

            public double Dot(Point other) {
                return X * other.X + Y * other.Y;
            }

            public double Distance(Point other) {
                return Math.Sqrt( Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2) );
            }

            public double Length() {
                return ZERO.Distance(this); // lol
            }

            public static (int, Point) FromBlock( string block ) {

                var match = Pattern().MatchOrElse(block, $"Malformed point: {block}");

                return (
                    int.Parse(match.Groups[1].Value),
                    new Point(
                        float.Parse(match.Groups[2].Value),
                        float.Parse(match.Groups[3].Value)
                    )
                );
            }

        }
    }
}
