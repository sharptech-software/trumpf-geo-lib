
using Fasteroid;
using System.Text.RegularExpressions;

namespace SharpTech {
    public partial class GEOLib {

        /// <summary>
        /// Creates a new, empty drawing
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public partial class Drawing(double width, double height) {

            [GeneratedRegex($@"^({RE.DEC}) ({RE.DEC}) {RE.DEC}"  , RegexOptions.Singleline | RegexOptions.Multiline)]
            private static partial Regex SizePattern();

            internal void AddEntities(List<string> blocks) {
                foreach(string block in blocks) {
                    try {
                        var ent = Entity.FromBlock(block, this);
                        if(ent != null) Entities.Add(ent);
                    }
                    catch(Exception e) {
                        Console.Error.WriteLine($"Error parsing entity: {e.Message}");
                    }
                }
            }

            internal Point LookupPoint(int idx) {
                return Points.GetOrElse(idx, $"Point {idx} not found");
            }

            /// <summary>
            /// Maps <b>Point ID → <see cref="Point"/></b>
            /// </summary>
            public readonly Dictionary<int, Point> Points = [];

            /// <summary>
            /// Maps <b>Attribute ID → <see cref="Attribute"/></b>
            /// </summary>
            public readonly Dictionary<int, Attribute> Attributes = [];

            /// <summary>
            /// A list of drawing <see cref="Entity">entities</see> in this drawing.
            /// </summary>
            public readonly List<Entity> Entities = [];

            /// <summary>
            /// Width of the drawing.
            /// </summary>
            public readonly double Width = width;

            /// <summary>
            /// Height of the drawing.
            /// </summary>
            public readonly double Height = height;

            /// <summary>
            /// Converts the drawing to SVG.
            /// </summary>
            /// <returns>This drawing represented as an SVG</returns>
            public SVG ToSVG() { 
                SVG svg = new(Width, Height);
                foreach(var ent in Entities) {
                    svg.Children.Add(ent);
                }
                return svg;
            }

            /// <summary>
            /// Creates a <see cref="Drawing"/> based on the provided GEO file.
            /// </summary>
            /// <param name="filepath">File to load</param>
            /// <returns>The drawing</returns>
            public static async Task< Drawing > FromFile(string filepath) {

                var pre = await Load(filepath);

                Drawing drawing;

                try {
                    string header = pre.GetOrElse(ENUMS.SECTION.HEADER, "GEO has no header")[0];

                    header.SkipLines(5).TakeLines(1, out string size);
                        var sizeMatch = SizePattern().MatchOrElse(size, "regex");
                        double width = double.Parse(sizeMatch.Groups[1].Value);
                        double height = double.Parse(sizeMatch.Groups[2].Value);

                    drawing = new Drawing(width, height);
                }
                catch(Exception e ) {
                    throw new InvalidDataException("GEO header was malformed", e);
                }


                foreach(string block in pre.GetValueOrDefault(ENUMS.SECTION.ATT, [])) {
                    try {
                        (int id, Attribute att) = Attribute.FromBlock(block);
                        drawing.Attributes.Add(id, att);
                    }
                    catch(Exception e) {
                        Console.Error.WriteLine($"Error parsing attribute: {e.Message}");
                    }
                }

                foreach(string block in pre.GetValueOrDefault(ENUMS.SECTION.POINTS, [])) {
                    try {
                        (int id, Point p) = Point.FromBlock(block);
                        drawing.Points.Add(id, p);
                    }
                    catch(Exception e) {
                        Console.Error.WriteLine($"Error parsing point: {e.Message}");
                    }
                }


                drawing.AddEntities(pre.GetValueOrDefault(ENUMS.SECTION.TEXT, []));
                drawing.AddEntities(pre.GetValueOrDefault(ENUMS.SECTION.ENTITIES, []));
                drawing.AddEntities(pre.GetValueOrDefault(ENUMS.SECTION.BEND_ENTITIES, []));

                return drawing;
            }

        }

    }

}
