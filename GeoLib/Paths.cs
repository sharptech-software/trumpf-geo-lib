#pragma warning disable 1591
using System.Text.RegularExpressions;

namespace SharpTech {
    public partial class GEOLib {

        public partial class Line : Entity, IStroke {

            [GeneratedRegex($@"^({RE.INT}) ({RE.INT})$", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public Point Start;
            public Point End;

            internal Line(ReadOnlySpan<char> entblock, Drawing parent) : base(ref entblock, parent, ENUMS.ENTITY.LINE) { 
                var match = Pattern().MatchOrElse(entblock.ToString(), $"Malformed line: {entblock}");
                Start   = parent.LookupPoint(int.Parse(match.Groups[1].Value));
                End     = parent.LookupPoint(int.Parse(match.Groups[2].Value));
            }

            // svg interface
            string IStroke.StrokeStart => $"M {Start.X} {Start.Y}";
            string IStroke.StrokeBody => $"L {End.X} {End.Y}";
        }


        public partial class Circle : Entity, IStroke {

            [GeneratedRegex($@"^({RE.INT})\r?\n({RE.DEC})$", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public Point Center;
            public double Radius;

            internal Circle(ReadOnlySpan<char> entblock, Drawing parent) : base(ref entblock, parent, ENUMS.ENTITY.CIRCLE) {
                var match = Pattern().MatchOrElse(entblock.ToString(), $"Malformed circle: {entblock}");
                Center = parent.LookupPoint(int.Parse(match.Groups[1].Value));
                Radius = double.Parse(match.Groups[2].Value);
            }

            // svg interface
            string IStroke.StrokeStart => $"M {Center.X} {Center.Y - Radius}";
            string IStroke.StrokeBody => $"a {Radius} {Radius} 180 1 0 0 {2 * Radius}\na {Radius} {Radius} 180 1 0 0 {-2 * Radius}";
        }

        public partial class Arc : Entity, IStroke {

            [GeneratedRegex($@"({RE.INT}) ({RE.INT}) ({RE.INT})(\r?\n-1)?$", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public Point Start;
            public Point Center;
            public Point End;
            public bool  Clockwise;

            public double Radius;

            internal Arc(ReadOnlySpan<char> entblock, Drawing parent) : base(ref entblock, parent, ENUMS.ENTITY.ARC) {
                var match = Pattern().MatchOrElse(entblock.ToString(), $"Malformed arc: {entblock}");
                Center    = parent.LookupPoint(int.Parse(match.Groups[1].Value));
                Start     = parent.LookupPoint(int.Parse(match.Groups[2].Value));
                End       = parent.LookupPoint(int.Parse(match.Groups[3].Value));
                Clockwise = match.Groups[4].Success;

                Radius    = Center.Distance(End);
            }

            // svg interface
            string IStroke.StrokeStart => $"M {Start.X} {Start.Y}";
            string IStroke.StrokeBody => $"A {Radius} {Radius} 0 0 {(Clockwise ? 1 : 0)} {End.X} {End.Y}";

        }

    }
}
