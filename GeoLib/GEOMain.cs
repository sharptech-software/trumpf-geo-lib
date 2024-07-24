using System.Text.RegularExpressions;
using static Fasteroid.GEOLib;

namespace Fasteroid {
    public partial class GEOLib {

        public interface IEntity {
            public Type Type { get; }
        }

        public partial class Point {

            [GeneratedRegex(@"(\d+)\n(\d+\.\d+) (\d+\.\d+) \d+\.\d+", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex Pattern();

            public readonly float X;
            public readonly float Y;

            private Point(float x, float y) {
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

        public partial class Line : IEntity {

            [GeneratedRegex(@"\d+ \d+\n(\d+) (\d+)$", RegexOptions.Singleline | RegexOptions.Multiline)]
            internal static partial Regex Pattern();

            public Type Type => typeof(Line);

            public readonly Point Start;
            public readonly Point End;

            public Line( Point start, Point end ) {
                Start = start;
                End = end;
            }

        }

        public partial class Circle : IEntity {

            [GeneratedRegex(@"^\d+ \d+\n(\d+)\n(\d+\.\d+)$", RegexOptions.Singleline | RegexOptions.Multiline)]
            internal static partial Regex Pattern();

            public Type Type => typeof(Circle);

            public readonly Point Center;
            public readonly float Radius;

            public Circle( Point center, float radius ) {
                Center = center;
                Radius = radius;
            }

        }

        public partial class Arc : IEntity {

            [GeneratedRegex(@"\d+ \d+\n(\d+) (\d+) (\d+)(\n-1)?$", RegexOptions.Singleline | RegexOptions.Multiline)]
            internal static partial Regex Pattern();

            public Type Type => typeof(Arc);

            public readonly Point Start;
            public readonly Point Center;
            public readonly Point End;
            public readonly bool  Clockwise;

            public Arc( Point center, Point start, Point end, bool clockwise ) {
                Start     = start;
                Center    = center;
                End       = end;
                Clockwise = clockwise;
            }

        }


        public partial class Drawing {

            [GeneratedRegex(@"^(.*?)\n", RegexOptions.Singleline)]
            private static partial Regex EntityTypePattern();

            public readonly Dictionary<int, Point> Points   = new Dictionary<int, Point>();
            public readonly List<IEntity>          Entities = new List<IEntity>();

            public Point LookupPoint(int idx) {
                return Points.GetOrElse(idx, $"Point {idx} not found");
            }

            public static async Task< Drawing > FromFile( string filepath ) {

                var drawing = new Drawing();

                var pre = await Load(filepath);

                foreach( string ptBlock in pre.GetValueOrDefault(TYPES.SECTION.POINTS, []) ) {
                    try {
                        (int id, Point p) = Point.FromBlock(ptBlock);
                        drawing.Points.Add(id, p);
                    }
                    catch( Exception e ) {
                        Console.Error.WriteLine($"Error parsing point: {e.Message}");
                    }
                }

                foreach( string entBlock in pre.GetValueOrDefault(TYPES.SECTION.ENTITIES, []) ) {

                    var entMatch = EntityTypePattern().MatchOrElse(entBlock, $"Malformed line: {entBlock}");;

                    try {
                        switch( entMatch.Groups[1].Value ) {
                            case TYPES.ENTITY.LINE:
                                var lineMatch = Line.Pattern().MatchOrElse(entBlock, $"Malformed line: {entBlock}");
                                var p1 = int.Parse(lineMatch.Groups[1].Value);
                                var p2 = int.Parse(lineMatch.Groups[2].Value);
                                drawing.Entities.Add( new Line(
                                    drawing.LookupPoint(p1),
                                    drawing.LookupPoint(p2)
                                ));
                            break;

                            case TYPES.ENTITY.CIRCLE:
                                var circleMatch = Circle.Pattern().MatchOrElse(entBlock, $"Malformed circle: {entBlock}");
                                var p = int.Parse(circleMatch.Groups[1].Value);
                                drawing.Entities.Add( new Circle(
                                    drawing.LookupPoint(p),
                                    float.Parse(circleMatch.Groups[2].Value)
                                ));
                            break;

                            case TYPES.ENTITY.ARC:
                                var arcMatch  = Arc.Pattern().MatchOrElse(entBlock, $"Malformed arc: {entBlock}");
                                var center    = int.Parse(arcMatch.Groups[1].Value);
                                var start     = int.Parse(arcMatch.Groups[2].Value);
                                var end       = int.Parse(arcMatch.Groups[3].Value);
                                var clockwise = arcMatch.Groups[4].Success;
                                drawing.Entities.Add( new Arc(
                                    drawing.LookupPoint(center),
                                    drawing.LookupPoint(start),
                                    drawing.LookupPoint(end),
                                    clockwise
                                ));
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
