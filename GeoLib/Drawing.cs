using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Fasteroid {
    public partial class GEOLib {
        public partial class Drawing {

            [GeneratedRegex(@"^ATT\n", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex AttTypePattern();

            [GeneratedRegex($@"^({RE.DEC}) ({RE.DEC}) {RE.DEC}", RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex SizePattern();

            public readonly Dictionary<int, Point>     Points = [];
            public readonly Dictionary<int, Attribute> Atts   = [];
            public readonly List<ISVGElement>          Paths  = [];
            public readonly Point                      Size;

            public Point LookupPoint(int idx) {
                return Points.GetOrElse(idx, $"Point {idx} not found");
            }

            public Drawing(float x, float y) {
                Size = new Point(x, y);
            }

            // easy construction 
            [GeneratedRegex(@"^(.*?)\n")]
            private static partial Regex EntityTypePattern();
            public static ISVGElement ConstructFromBlock( string block, Drawing parent ) {
                var match = EntityTypePattern().MatchOrElse(block, $"Malformed entity: {block}");

                switch( match.Groups[1].Value ) {
                    case CONSTANTS.ENTITY.LINE:
                        return new Line(block, parent);
                    case CONSTANTS.ENTITY.CIRCLE:
                        return new Circle(block, parent);
                    case CONSTANTS.ENTITY.ARC:
                        return new Arc(block, parent);
                    default:
                        throw new Exception($"Unknown entity type: {match.Groups[1].Value}");
                }
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
                    try {
                        drawing.Paths.Add( ConstructFromBlock(entityBlock, drawing) );
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
