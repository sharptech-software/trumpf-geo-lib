using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {

        public abstract partial class Entity {

            [GeneratedRegex($@"^({RE.INT}) ({RE.INT})$", RegexOptions.Singleline)]
            protected static partial Regex AppearancePattern();

            [GeneratedRegex($@"({RE.INT})\n({RE.INT})$", RegexOptions.Singleline)]
            protected static partial Regex AttPattern();

            public abstract Type Type   { get; }

            public virtual int   Color      { get; }
            public virtual int   Stroke     { get; }

            protected virtual bool ParseAttByDefault => true;
            public virtual int? AttID { get; }

            public Entity( string block ): this(block, out string _) { }

            public Entity( string block, out string the_rest ) {
                the_rest = block.TakeLines(1, out string _) // skip the type line
                                .TakeLines(1, out string appearance);

                var appearanceMatch = AppearancePattern().MatchOrElse(appearance, $"Malformed entity: {appearance}");
                Color  = int.Parse(appearanceMatch.Groups[1].Value);
                Stroke = int.Parse(appearanceMatch.Groups[2].Value);

                if( ParseAttByDefault ) {
                    var attMatch = AttPattern().Match(the_rest);
                    AttID = attMatch.Success ? int.Parse(attMatch.Groups[1].Value) : null;
                }
            }

            // svg interface
            public virtual string  PathColor => CONSTANTS.COLORS.Lookup(Color);
            public virtual string? PathDashPattern => CONSTANTS.STROKES.Lookup(Stroke);
        }


        public partial class Line : Entity, ISVGPath {

            [GeneratedRegex($@"^({RE.INT}) ({RE.INT})$", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public override Type Type => typeof(Line);

            public readonly Point Start;
            public readonly Point End;

            internal Line( string baseblock, Drawing parent ) : base( baseblock, out string continued ) { 
                var match = Pattern().MatchOrElse(continued, $"Malformed line: {continued}");
                Start   = parent.LookupPoint(int.Parse(match.Groups[1].Value));
                End     = parent.LookupPoint(int.Parse(match.Groups[2].Value));
            }

            // svg interface
            public string PathInstructions => $"M {Start.X} {Start.Y} L {End.X} {End.Y}";
        }


        public partial class Circle : Entity, ISVGPath {

            [GeneratedRegex($@"^({RE.INT})\n({RE.DEC})$", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public override Type Type => typeof(Circle);

            public readonly Point Center;
            public readonly float Radius;

            internal Circle( string baseblock, Drawing parent ) : base(baseblock, out string continued) {
                var match = Pattern().MatchOrElse(continued, $"Malformed circle: {continued}");
                Center = parent.LookupPoint(int.Parse(match.Groups[1].Value));
                Radius = float.Parse(match.Groups[2].Value);
            }

            // svg interface
            public string PathInstructions => $"M {Center.X} {Center.Y - Radius} a {Radius} {Radius} 180 1 0 0 {2 * Radius}\na {Radius} {Radius} 180 1 0 0 {-2 * Radius}";
        }

        public partial class Arc : Entity, ISVGPath {

            [GeneratedRegex($@"({RE.INT}) ({RE.INT}) ({RE.INT})(\n-1)?$", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public override Type Type => typeof(Arc);

            public readonly Point Start;
            public readonly Point Center;
            public readonly Point End;
            public readonly bool  Clockwise;

            public readonly double Radius;

            internal Arc( string baseblock, Drawing parent ) : base(baseblock, out string continued) {
                var match = Pattern().MatchOrElse(continued, $"Malformed arc: {continued}");
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
