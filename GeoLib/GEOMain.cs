using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {

        public interface IEntity {
            public Type Type { get; }
        }

        public partial class Point {

            [GeneratedRegex(@" (\d*) (\d*\.\d*) (\d*\.\d*)", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex PointPattern();

            public required float X;
            public required float Y;

            private Point() { }

            public static (int, Point) FromBlock( string block ) {

                var match = PointPattern().MatchOrElse(block, $"Malformed point: {block}");

                return (
                    int.Parse(match.Groups[1].Value),
                    new Point {
                        X = float.Parse(match.Groups[2].Value),
                        Y = float.Parse(match.Groups[3].Value)
                    }
                );
            }

        }

        public partial class Line : IEntity {
            public Type Type => typeof(Line);

            public readonly Point Start;
            public readonly Point End;

            public Line( Point start, Point end ) {
                Start = start;
                End = end;
            }
        }

        public partial class Drawing {

            [GeneratedRegex(@"^(.*?) ", RegexOptions.Singleline)]
            private static partial Regex EntityTypePattern();

            [GeneratedRegex(@"^.*? \d+ \d+ (\d+) (\d+)", RegexOptions.Singleline)]
            private static partial Regex LinePattern();


            public readonly Dictionary<int, Point> Points   = new Dictionary<int, Point>();
            public readonly List<IEntity>          Entities = new List<IEntity>();

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
                                var lineMatch = LinePattern().MatchOrElse(entBlock, $"Malformed line: {entBlock}");
                                var p1 = int.Parse(lineMatch.Groups[1].Value);
                                var p2 = int.Parse(lineMatch.Groups[2].Value);
                                Line l = new Line(
                                    drawing.Points.GetOrElse(p1, $"Point {p1} not found"),
                                    drawing.Points.GetOrElse(p2, $"Point {p2} not found")
                                );
                                drawing.Entities.Add(l);
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
