using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {

        public partial class Line : Entity, ISVGPath {

            [GeneratedRegex($@"^({RE.INT}) ({RE.INT})$", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public readonly Point Start;
            public readonly Point End;

            internal Line( ReadOnlySpan<char> entblock, Drawing parent ) : base( entblock, parent, CONSTANTS.ENTITY.LINE, out ReadOnlySpan<char> entdata ) { 
                var match = Pattern().MatchOrElse(entdata.ToString(), $"Malformed line: {entdata}");
                Start   = parent.LookupPoint(int.Parse(match.Groups[1].Value));
                End     = parent.LookupPoint(int.Parse(match.Groups[2].Value));
            }

            // svg interface
            public string PathInstructions => $"M {Start.X} {Start.Y} L {End.X} {End.Y}";
        }


        public partial class Circle : Entity, ISVGPath {

            [GeneratedRegex($@"^({RE.INT})\n({RE.DEC})$", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public readonly Point Center;
            public readonly float Radius;

            internal Circle( ReadOnlySpan<char> entblock, Drawing parent ) : base( entblock, parent, CONSTANTS.ENTITY.CIRCLE, out ReadOnlySpan<char> entdata ) {
                var match = Pattern().MatchOrElse(entdata.ToString(), $"Malformed circle: {entdata}");
                Center = parent.LookupPoint(int.Parse(match.Groups[1].Value));
                Radius = float.Parse(match.Groups[2].Value);
            }

            // svg interface
            public string PathInstructions => $"M {Center.X} {Center.Y - Radius} a {Radius} {Radius} 180 1 0 0 {2 * Radius}\na {Radius} {Radius} 180 1 0 0 {-2 * Radius}";
        }

        public partial class Arc : Entity, ISVGPath {

            [GeneratedRegex($@"({RE.INT}) ({RE.INT}) ({RE.INT})(\n-1)?$", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public readonly Point Start;
            public readonly Point Center;
            public readonly Point End;
            public readonly bool  Clockwise;

            public readonly double Radius;

            internal Arc( ReadOnlySpan<char> entblock, Drawing parent ) : base( entblock, parent, CONSTANTS.ENTITY.ARC, out ReadOnlySpan<char> entdata ) {
                var match = Pattern().MatchOrElse(entdata.ToString(), $"Malformed arc: {entdata}");
                Start     = parent.LookupPoint(int.Parse(match.Groups[1].Value));
                Center    = parent.LookupPoint(int.Parse(match.Groups[2].Value));
                End       = parent.LookupPoint(int.Parse(match.Groups[3].Value));
                Clockwise = match.Groups[4].Success;

                Radius    = Center.Distance(End);
            }

            // svg interface
            public string PathInstructions => $"M {Start.X} {Start.Y} A {Radius} {Radius} 0 0 {(Clockwise ? 1 : 0)} {End.X} {End.Y}";

        }

    }
}
