
using Fasteroid;
using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {
        public partial class Drawing {

            [GeneratedRegex($@"^({RE.DEC}) ({RE.DEC}) {RE.DEC}", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex SizePattern();

            public readonly Dictionary<int, Point>     Points    = [];
            public readonly Dictionary<int, Attribute> Atts      = [];
            public readonly List<Entity>               Entities  = [];
            public readonly Point                      Size;

            public Point LookupPoint(int idx) {
                return Points.GetOrElse(idx, $"Point {idx} not found");
            }

            public Drawing(float x, float y) {
                Size = new Point(x, y);
            }

            public Drawing(string header) {
                try {
                    header.SkipLines(5).TakeLines(1, out string size);
                    var sizeMatch = SizePattern().MatchOrElse( size, "regex" );

                    Size = new Point(
                        float.Parse( sizeMatch.Groups[1].Value ),
                        float.Parse( sizeMatch.Groups[2].Value )
                    );
                }
                catch( Exception e ) {
                    throw new FileLoadException("Malformed GEO header", e);
                }
            }

            public static async Task< Drawing > FromFile( string filepath ) {

                var pre = await Load(filepath);

                var drawing = new Drawing( pre.GetOrElse(CONSTANTS.SECTION.HEADER, "GEO has no header")[0] );

                foreach( string block in pre.GetValueOrDefault(CONSTANTS.SECTION.ATT, []) ) {
                    try {
                        (int id, Attribute att) = Attribute.FromBlock(block);
                        drawing.Atts.Add(id, att);
                    }
                    catch( Exception e ) {
                        Console.Error.WriteLine($"Error parsing attribute: {e.Message}");
                    }
                }

                foreach( string block in pre.GetValueOrDefault(CONSTANTS.SECTION.POINTS, []) ) {
                    try {
                        (int id, Point p) = Point.FromBlock(block);
                        drawing.Points.Add(id, p);
                    }
                    catch( Exception e ) {
                        Console.Error.WriteLine($"Error parsing point: {e.Message}");
                    }
                }

                foreach( string block in pre.GetValueOrDefault(CONSTANTS.SECTION.ENTITIES, []) ) {
                    try {
                        var ent = Entity.FromBlock(block, drawing);
                        if( ent != null ) drawing.Entities.Add( ent );
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
