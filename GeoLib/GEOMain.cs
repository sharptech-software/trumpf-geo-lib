using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {

        public partial class Point {

            [GeneratedRegex(@"P\n(\d*)\n(\d*\.\d*) (\d*\.\d*)", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex PointPattern();

            public int ID;

            public float X;
            public float Y;

            public Point( string line ) {
                Console.WriteLine(line);
                var match = PointPattern().Match(line);

                if( !match.Success )
                    throw new InvalidDataException($"Tried to parse a non-point as a point??! ({line})");

                ID = int.Parse(match.Groups[1].Value); // TODO: why are we storing this at all?  wouldn't a dictionary be better?
                X  = float.Parse(match.Groups[2].Value);
                Y  = float.Parse(match.Groups[3].Value);
            }

        }

        public class Drawing {

            public readonly List<Point> Points = new List<Point>();

            public static async Task< Drawing > FromFile( string filepath ) {
                var drawing = new Drawing();

                var pre = await Load(filepath);

                foreach( var section in pre ) {
                    switch( section.Key ) {
                        case (int) TYPES.SECTION.POINTS:
                            foreach( var point in section.Value ) {
                                drawing.Points.Add( new Point(point) );
                            }
                        break;
                    }
                }

                return drawing;
            }

        }

    }
}
