using System.Text.RegularExpressions;
using static Fasteroid.GEOLib;

namespace Fasteroid {
    public partial class GEOLib {

        public interface IEntity {
            public Type Type   { get; }

            public int Color   { get; }
            public int Stroke  { get; }
        }

        public partial class Point {

            [GeneratedRegex(@"(\d+)\n(\d+\.\d+) (\d+\.\d+) \d+\.\d+", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public readonly float X;
            public readonly float Y;

            public Point(float x, float y) {
                X = x;
                Y = y;
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

        public abstract partial class Entity : IEntity {
            [GeneratedRegex(@"^(\d+) (\d+)$", RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public abstract Type Type { get; }

            private readonly int _color;
            public int Color => _color;

            private readonly int _stroke;
            public int Stroke => _stroke;

            public Entity( string block ) {
                var match = Pattern().MatchOrElse(block, $"Malformed base entity: {block}");
                _color  = int.Parse(match.Groups[1].Value);
                _stroke = int.Parse(match.Groups[2].Value);
            }

            public Entity( int color, int stroke ) {
                _color  = color;
                _stroke = stroke;
            }
        }

        public partial class Line : Entity {

            [GeneratedRegex(@"\d+ \d+\n(\d+) (\d+)$", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public override Type Type => typeof(Line);

            public readonly Point Start;
            public readonly Point End;


            internal Line( string block, Drawing parent ) : base( block ) { 
                var match = Pattern().MatchOrElse(block, $"Malformed line: {block}");
                Start   = parent.LookupPoint(int.Parse(match.Groups[1].Value));
                End     = parent.LookupPoint(int.Parse(match.Groups[2].Value));
            }

            public Line( int color, int stroke, Point start, Point end ) : base(color, stroke) {
                Start   = start;
                End     = end;
            }

        }

        public partial class Circle : Entity {

            [GeneratedRegex(@"^\d+ \d+\n(\d+)\n(\d+\.\d+)$", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public override Type Type => typeof(Circle);

            public readonly Point Center;
            public readonly float Radius;

            internal Circle( string block, Drawing parent ) : base(block) {
                var match = Pattern().MatchOrElse(block, $"Malformed circle: {block}");
                Center = parent.LookupPoint(int.Parse(match.Groups[1].Value));
                Radius = float.Parse(match.Groups[2].Value);
            }

            public Circle( int color, int stroke, Point center, float radius ) : base(color, stroke) {
                Center = center;
                Radius = radius;
            }

        }

        public partial class Arc : Entity {

            [GeneratedRegex(@"\d+ \d+\n(\d+) (\d+) (\d+)(\n-1)?$", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public override Type Type => typeof(Arc);

            public readonly Point Start;
            public readonly Point Center;
            public readonly Point End;
            public readonly bool  Clockwise;

            internal Arc( string block, Drawing parent ) : base(block) {
                var match = Pattern().MatchOrElse(block, $"Malformed arc: {block}");
                Start     = parent.LookupPoint(int.Parse(match.Groups[1].Value));
                Center    = parent.LookupPoint(int.Parse(match.Groups[2].Value));
                End       = parent.LookupPoint(int.Parse(match.Groups[3].Value));
                Clockwise = match.Groups[4].Success;
            }

            public Arc( int color, int stroke, Point center, Point start, Point end, bool clockwise ) : base(color, stroke) {
                Start     = start;
                Center    = center;
                End       = end;
                Clockwise = clockwise;
            }

        }

        public partial class Text : Entity {

            // The regex below could be used for multi-line text, but in practice it looks like nobody ever does that:
            // \d+ \d+\n(\d+)\n\d+\.\d+ \d+\.\d+ \d+\.\d+\n\d+\.\d+ \d+\.\d+\n\d+ \d+ \d+\n(?>(?>(.*)\n\d+\n\d+)|([^\n]*))
            // Perhaps someday this will be useful.  Looks like it only shows up if the file version (line 3) == 1

            [GeneratedRegex(@"\d+ \d+\n(\d+)\n\d+\.\d+ \d+\.\d+ \d+\.\d+\n\d+\.\d+ \d+\.\d+\n\d+ \d+ \d+\n([^\n]*)", RegexOptions.Singleline | RegexOptions.Multiline)]
            internal static partial Regex Pattern();

            public override Type Type => typeof(Text);

            public readonly Point  Origin;
            public readonly string Content;

            internal Text( string block, Drawing parent ) : base(block) {
                var match = Pattern().MatchOrElse(block, $"Malformed text: {block}");
                Origin  = parent.LookupPoint(int.Parse(match.Groups[1].Value));
                Content = match.Groups[2].Value;
            }

            public Text( int color, int stroke, Point origin, string content ) : base(color, stroke) {
                Origin  = origin;
                Content = content;
            }

        }


        public partial class Drawing {

            [GeneratedRegex(@"^(.*?)\n", RegexOptions.Singleline)]
            private static partial Regex EntityTypePattern();

            [GeneratedRegex(@"^(\d+\.\d+) (\d+\.\d+) \d+\.\d+", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex SizePattern();


            public readonly Dictionary<int, Point> Points   = [];
            public readonly List<IEntity>          Entities = [];
            public readonly Point                  Size;

            public Point LookupPoint(int idx) {
                return Points.GetOrElse(idx, $"Point {idx} not found");
            }

            public Drawing(float x, float y) {
                Size = new Point(x, y);
            }

            public static async Task< Drawing > FromFile( string filepath ) {

                var pre = await Load(filepath);

                var headerLines = pre.GetOrElse(TYPES.SECTION.HEADER, "GEO has no header").First().Split('\n');
                var sizeMatch = SizePattern().MatchOrElse( headerLines.ElementAtOrDefault(5) ?? "", "Malformed GEO header" );

                var drawing = new Drawing(
                    float.Parse( sizeMatch.Groups[1].Value ),
                    float.Parse( sizeMatch.Groups[2].Value )
                );

                foreach( string ptBlock in pre.GetValueOrDefault(TYPES.SECTION.POINTS, []) ) {
                    try {
                        (int id, Point p) = Point.FromBlock(ptBlock);
                        drawing.Points.Add(id, p);
                    }
                    catch( Exception e ) {
                        Console.Error.WriteLine($"Error parsing point: {e.Message}");
                    }
                }

                foreach( string txtBlock in pre.GetValueOrDefault(TYPES.SECTION.TEXT, []) ) {
                    drawing.Entities.Add( new Text(txtBlock, drawing) );
                }

                foreach( string entBlock in pre.GetValueOrDefault(TYPES.SECTION.ENTITIES, []) ) {

                    var entMatch = EntityTypePattern().MatchOrElse(entBlock, $"Malformed line: {entBlock}");

                    try {
                        switch( entMatch.Groups[1].Value ) {
                            case TYPES.ENTITY.LINE:
                                drawing.Entities.Add( new Line(entBlock, drawing) );
                            break;

                            case TYPES.ENTITY.CIRCLE:
                                drawing.Entities.Add( new Circle(entBlock, drawing) );
                            break;

                            case TYPES.ENTITY.ARC:
                                drawing.Entities.Add( new Arc(entBlock, drawing) );
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
