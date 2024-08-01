using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {


        public partial class Point {

            public readonly static Point ZERO = new Point(0,0);

            [GeneratedRegex($@"({RE.INT})\n({RE.DEC}) ({RE.DEC}) {RE.DEC}", RegexOptions.Singleline | RegexOptions.Multiline)]
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

        public abstract class Att {
            public abstract Type Type { get; }
        }

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
            public double   Length { get; }

            private double _ArcLength() {
                Point v1 = Start - Center;
                Point v2 = End - Center;

                double dot = v1.Dot(v2);
                double ang = Math.Acos(dot / (v1.Length() * v2.Length()));

                return ang * Radius;
            }

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

        public partial class Drawing {

            [GeneratedRegex(@"^(.*?)\n")]
            private static partial Regex ContourTypePattern();

            [GeneratedRegex(@"^ATT\n", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex AttTypePattern();

            [GeneratedRegex($@"^({RE.DEC}) ({RE.DEC}) {RE.DEC}", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex SizePattern();

            public readonly Dictionary<int, Point> Points = [];
            public readonly Dictionary<int, Att>   Atts   = [];
            public readonly List<ISVGPath>         Paths  = [];
            public readonly Point                  Size;

            public Point LookupPoint(int idx) {
                return Points.GetOrElse(idx, $"Point {idx} not found");
            }

            public Drawing(float x, float y) {
                Size = new Point(x, y);
            }

            public static async Task< Drawing > FromFile( string filepath ) {

                var pre = await Load(filepath);

                var headerLines = pre.GetOrElse(CONSTANTS.SECTION.HEADER, "GEO has no header")[0].Split('\n');
                var sizeMatch = SizePattern().MatchOrElse( headerLines.ElementAtOrDefault(5) ?? "", "Malformed GEO header" );

                var drawing = new Drawing(
                    float.Parse( sizeMatch.Groups[1].Value ),
                    float.Parse( sizeMatch.Groups[2].Value )
                );

                foreach( string ptBlock in pre.GetValueOrDefault(CONSTANTS.SECTION.POINTS, []) ) {
                    try {
                        (int id, Point p) = Point.FromBlock(ptBlock);
                        drawing.Points.Add(id, p);
                    }
                    catch( Exception e ) {
                        Console.Error.WriteLine($"Error parsing point: {e.Message}");
                    }
                }

                foreach( string entityBlock in pre.GetValueOrDefault(CONSTANTS.SECTION.ENTITIES, []) ) {

                    var entMatch = ContourTypePattern().MatchOrElse(entityBlock, $"Malformed entity: {entityBlock}");

                    try {
                        switch( entMatch.Groups[1].Value ) {
                            case CONSTANTS.ENTITY.LINE:
                                drawing.Paths.Add( new Line(entityBlock, drawing) );
                            break;

                            case CONSTANTS.ENTITY.CIRCLE:
                                drawing.Paths.Add( new Circle(entityBlock, drawing) );
                            break;

                            case CONSTANTS.ENTITY.ARC:
                                drawing.Paths.Add( new Arc(entityBlock, drawing) );
                            break;

                            default:
                                Console.Error.WriteLine($"Unknown entity type: {entMatch.Groups[1].Value}");
                            break;
                        }
                    }
                    catch( Exception e ) {
                        Console.Error.WriteLine($"Error parsing entity: {e.Message}");
                    }

                }
                return drawing;
            }

        }

    }
}
