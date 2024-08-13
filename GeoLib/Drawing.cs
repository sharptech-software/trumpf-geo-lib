
using Fasteroid;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SharpTech {
    public partial class GEOLib {
        public partial class Drawing {

            [GeneratedRegex($@"^({RE.DEC}) ({RE.DEC}) {RE.DEC}"  , RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex SizePattern();

            public readonly Dictionary<int, Point>     Points    = [];
            public readonly Dictionary<int, Attribute> Atts      = [];
            public readonly List<Entity>               Entities  = [];
            public readonly Point                      Size;

            public Point LookupPoint(int idx) {
                return Points.GetOrElse(idx, $"Point {idx} not found"  );
            }

            public Drawing(double x, double y) {
                Size = new Point(x, y);
            }

            public Drawing(string header) {

                // debug: enumerate resources
                var assembly = Assembly.GetExecutingAssembly();
                foreach(var res in assembly.GetManifestResourceNames()) {
                    Console.WriteLine(res);
                }
                

                try {
                    header.SkipLines(5).TakeLines(1, out string size);
                    var sizeMatch = SizePattern().MatchOrElse(size, "regex"  );

                    Size = new Point(
                        double.Parse(sizeMatch.Groups[1].Value),
                        double.Parse(sizeMatch.Groups[2].Value)
                 );
                }
                catch(Exception e) {
                    throw new FileLoadException("Malformed GEO header"  , e);
                }
            }

            public SVG ToSVG() { 
                SVG svg = new(Size.X, Size.Y);
                foreach(var ent in Entities) {
                    svg.Children.Add(ent);
                }
                return svg;
            }

            internal void AddEntities(List<string> blocks) {
                foreach(string block in blocks) {
                    try {
                        var ent = Entity.FromBlock(block, this);
                        if(ent != null) Entities.Add(ent);
                    }
                    catch(Exception e) {
                        Console.Error.WriteLine($"Error parsing entity: {e.Message}"  );
                    }
                }
            }

            public static async Task< Drawing > FromFile(string filepath) {

                var pre = await Load(filepath);

                var drawing = new Drawing(pre.GetOrElse(ENUMS.SECTION.HEADER, "GEO has no header")[0]);

                foreach(string block in pre.GetValueOrDefault(ENUMS.SECTION.ATT, [])) {
                    try {
                        (int id, Attribute att) = Attribute.FromBlock(block);
                        drawing.Atts.Add(id, att);
                    }
                    catch(Exception e) {
                        Console.Error.WriteLine($"Error parsing attribute: {e.Message}"  );
                    }
                }

                foreach(string block in pre.GetValueOrDefault(ENUMS.SECTION.POINTS, [])) {
                    try {
                        (int id, Point p) = Point.FromBlock(block);
                        drawing.Points.Add(id, p);
                    }
                    catch(Exception e) {
                        Console.Error.WriteLine($"Error parsing point: {e.Message}"  );
                    }
                }


                // drawing.AddEntities(pre.GetValueOrDefault(ENUMS.SECTION.TEXT, []));
                drawing.AddEntities(pre.GetValueOrDefault(ENUMS.SECTION.ENTITIES, []));
                drawing.AddEntities(pre.GetValueOrDefault(ENUMS.SECTION.BEND_ENTITIES, []));

                return drawing;
            }

        }
    }
}
