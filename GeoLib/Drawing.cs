
using Fasteroid;
using System.Diagnostics;
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

            internal void AddGroups(List< List<string> > groups) {
                foreach( var group in groups) {
                    var ents = new List<Entity>();
                    foreach( var block in group ) {
                        try {
                            var ent = Entity.FromBlock(block, this);
                            ents.Add(ent);
                        }
                        catch( Exception e ) {
                            Console.Error.WriteLine($"Error parsing entity: {e.Message}");
                        }
                    }
                    Groups.Add(ents);
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
            /// A list of grouped drawing <see cref="Entity">entities</see> in this drawing.
            /// </summary>
            public readonly List< List<Entity> > Groups = [];


            /// <summary>
            /// Width of the drawing.
            /// </summary>
            public double Width = width;

            /// <summary>
            /// Height of the drawing.
            /// </summary>
            public double Height = height;

            /// <summary>
            /// The hex color to use for entities with the <see cref="ENUMS.COLORS.DEFAULT">default color</see><br/>
            /// (default: <c>"#000"</c>)<br/>
            /// <br/>
            /// <c>null</c> = no predefined stroke color
            /// </summary>
            public string? StrokeColor = "#000";

            /// <summary>
            /// If set, adds a closed path to the SVG for filling it in.<br/>
            /// (default: <c>"background"</c>)<br/>
            /// <br/>
            /// <c>null</c> = don't add the background fill path
            /// </summary>
            public string? FillClass = "background";

            /// <summary>
            /// Hex color to use for the <see cref="FillClass">background fill</see>.<br/>
            /// (default: <c>"#777"</c>)<br/>
            /// <br/>
            /// <c>null</c> = no predefined fill color<br/>
            /// </summary>
            public string? FillColor = "#777";

            /// <summary>
            /// If true (default), the converted SVG will scale to the width of its container.<br/>
            /// </summary>
            public bool Responsive = true;

            /// <summary>
            /// Converts the drawing to SVG.
            /// </summary>
            /// <returns>This drawing represented as an SVG</returns>
            public SVG ToSVG() { 
                SVG svg = new(Width, Height, StrokeColor, Responsive);

                if( FillClass != null ) {
                    Contour contour = new(
                        Groups.Where(
                            group => group.All( 
                                ent => ent.Color == ENUMS.COLORS.DEFAULT && 
                                ent.Attribute?.Type != ENUMS.ATTRIBUTE.TEXT_SLAVE // these may look like contour components but they AREN'T!!
                            )
                        )
                        .Select(
                            group => group.ToStrokes()
                        )
                        .Where(
                            group => group.Any()
                        ),
                        FillClass,
                        FillColor,
                        StrokeColor
                    );


                    svg.Children.Add(contour);
                }

                foreach( var group in Groups ) {
                    foreach( var ent in group ) {
                        svg.Children.Add(ent);
                    }
                }

                return svg;
            }

            /// <summary>
            /// Creates a <see cref="Drawing"/> based on the provided GEO file path.
            /// </summary>
            /// <param name="filepath">File to load</param>
            /// <returns>The drawing</returns>
            public static async Task< Drawing > FromFile(string filepath) {
                return Drawing.FromCommon(Load(await File.ReadAllTextAsync(filepath)));
            }

            /// <summary>
            /// Creates a <see cref="Drawing"/> based on the provided GEO file bytes.
            /// </summary>
            /// <param name="bytes"></param>
            /// <returns>the drawing</returns>
            public static Drawing FromFileBytes(byte[] bytes)
            {
                return Drawing.FromCommon(Load(System.Text.Encoding.UTF8.GetString(bytes)));
            }

            internal static Drawing FromCommon(Dictionary<int, List< List<string> >> pre)
            {
                Drawing drawing;


                try
                {
                    string header = pre.GetOrElse(ENUMS.SECTION.HEADER, "GEO has no header")[0][0];

                    header.SkipLines(5).TakeLines(1, out string size);
                    var sizeMatch = SizePattern().MatchOrElse(size, "regex");
                    double width = double.Parse(sizeMatch.Groups[1].Value);
                    double height = double.Parse(sizeMatch.Groups[2].Value);

                    drawing = new Drawing(width, height);
                }
                catch (Exception e)
                {
                    throw new InvalidDataException("GEO header was malformed", e);
                }


                foreach (string block in pre.GetValueOrDefault(ENUMS.SECTION.ATT, []).SelectMany( x => x ))
                {
                    try
                    {
                        (int id, Attribute att) = Attribute.FromBlock(block);
                        drawing.Attributes.Add(id, att);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine($"Error parsing attribute: {e.Message}");
                    }
                }

                foreach (string block in pre.GetValueOrDefault(ENUMS.SECTION.POINTS, []).SelectMany( x => x ))
                {
                    try
                    {
                        (int id, Point p) = Point.FromBlock(block);
                        drawing.Points.Add(id, p);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine($"Error parsing point: {e.Message}");
                    }
                }


                drawing.AddGroups( pre.GetValueOrDefault(ENUMS.SECTION.TEXT, []) );
                drawing.AddGroups( pre.GetValueOrDefault(ENUMS.SECTION.BEND_ENTITIES, []) );
                drawing.AddGroups( pre.GetValueOrDefault(ENUMS.SECTION.ENTITIES, []) );

                return drawing;
            }


        }

    }

}
