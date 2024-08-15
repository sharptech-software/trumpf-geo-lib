

using System.Text;
using System.Text.RegularExpressions;
using Fasteroid;

namespace SharpTech {
    public partial class GEOLib {

        /// <summary>
        /// A "normal" GEO text entity (IE, not circular text)
        /// </summary>
        public partial class Text : Entity, ISVGElement {

            internal class GlyphRef(Font.Glyph glyph, Point position, string color) : ISVGElement {
                public readonly Font.Glyph Glyph    = glyph;
                public readonly Point      Position = position;
                public readonly string     Color    = color;

                public string ToSVGElement(SVG parent) {
                    if( parent.AllocateSharedFeature(Glyph.Name) ) { // ensure reference is available
                        parent.Children.Add(Glyph);
                    }
                    return $@"<use href='#{Glyph.Name}' x='{Position.X}' y='{Position.Y}' stroke='{Color}'/>";
                }
            }

            #pragma warning disable 1591
            /// <summary>
            /// Enums for text alignment.
            /// </summary>
            public static class ALIGN {
                public const int AFTER  = 0b100;
                public const int MIDDLE = 0b010;
                public const int BEFORE = 0b001;
            }

            #pragma warning disable 1591
            /// <summary>
            /// Enums for text writing direction.
            /// </summary>
            public static class DIRECTION {
                public const int LEFT = 0b001;
                public const int DOWN = 0b100;
            }

            [GeneratedRegex($@"^({RE.DEC}) ({RE.DEC}) ({RE.DEC})", RegexOptions.Singleline)]
            private static partial Regex Part1Pattern();

            [GeneratedRegex($@"^({RE.DEC}) ({RE.DEC})", RegexOptions.Singleline)]
            private static partial Regex Part2Pattern();

            [GeneratedRegex($@"^({RE.INT}) ({RE.INT}) ({RE.INT})", RegexOptions.Singleline)]
            private static partial Regex Part3Pattern();

            /// <summary>
            /// Text has to set <see cref="Attribute"/> in its own constructor.<br/>
            /// This overridden method returns null so we can do that.
            /// </summary>
            /// <param name="entdata"></param>
            protected override Attribute? GetAttFromData(ref ReadOnlySpan<char> entdata) => null;

            /// <inheritdoc/>
            public override Attribute? Attribute { get; } // we'll set this up here instead

            /// <summary>
            /// The "origin" point of the text.<br/>
            /// It will spill off of this point according to its <see cref="VAlign"/> and <see cref="HAlign"/>.
            /// </summary>
            public Point  Origin      { get; }

            /// <summary>
            /// Font size.
            /// </summary>
            public double  LineHeight  { get; }

            /// <summary>
            /// Width-to-height ratio of the text.<br/>
            /// Changes the scale of the text horizontally.
            /// </summary>
            public double  WHRatio     { get; }

            /// <summary>
            /// How "italic" the text is in degrees.
            /// </summary>
            public double  Inclination { get; }

            /// <summary>
            /// Line spacing as described in the GEO file.<br/>
            /// </summary>
            public double  LineSpacing { get; }

            /// <summary>
            /// Rotation of the text in degrees.
            /// </summary>
            public double  Angle       { get; }

            /// <summary>
            /// Vertical alignment of the text.<br/>
            /// See values in <see cref="ALIGN"/> for details.
            /// </summary>
            public int VAlign   { get; }

            /// <summary>
            /// Horizontal alignment of the text.<br/>
            /// See values in <see cref="ALIGN"/> for details.
            /// </summary>
            public int HAlign { get; }

            /// <summary>
            /// Writing direction of the text.<br/>
            /// See values in <see cref="DIRECTION"/> for details.
            /// </summary>
            public int WriteDir { get; }

            /// <summary>
            /// The actual text content.
            /// </summary>
            public string InnerText { get; }

            internal readonly Font Font;

            internal Text(ReadOnlySpan<char> textblock, Drawing parent) : base(ref textblock, parent, ENUMS.ENTITY.CIRCLE) {

                textblock = textblock.TakeLines(1, out string origin)
                                     .TakeLines(1, out string part1)
                                     .TakeLines(1, out string part2)
                                     .TakeLines(1, out string part3);

                Origin = parent.LookupPoint( int.Parse(origin) );

                Font = FONTS.Cache.GetOrElse(Stroke, "Text entity had unknown font");

                var part1Match = Part1Pattern().MatchOrElse(part1, $"Malformed text section 1: {part1}");
                var part2Match = Part2Pattern().MatchOrElse(part2, $"Malformed text section 2: {part2}");
                var part3Match = Part3Pattern().MatchOrElse(part3, $"Malformed text section 3: {part3}");

                LineHeight  = double.Parse(part1Match.Groups[1].Value);
                WHRatio     = double.Parse(part1Match.Groups[2].Value);
                Inclination = double.Parse(part1Match.Groups[3].Value);

                LineSpacing = double.Parse(part2Match.Groups[1].Value);
                Angle       = double.Parse(part2Match.Groups[2].Value);

                WriteDir    = int.Parse(part3Match.Groups[2].Value);

                {
                    int align = int.Parse(part3Match.Groups[1].Value);
                        VAlign = align & 0b111;
                        HAlign = align >> 3;
                }

                {
                    int numOfLines = int.Parse(part3Match.Groups[3].Value);
                        textblock = textblock.TakeLines(numOfLines, out string text);
                        InnerText = text.Replace("\r\n","\n");
                }

                try {
                    Attribute = base.GetAttFromData(ref textblock);
                }
                catch {
                    Attribute = null;
                }
            }

            /// <summary>
            /// The text content split into lines.<br/>
            /// If the text's direction is <see cref="DIRECTION.DOWN"/>, this will be a single character per line.
            /// </summary>
            public string[] Lines { get {
                if( WriteDir == DIRECTION.LEFT ) {
                    return InnerText.Split('\n');
                }
                else {
                    return InnerText.Select(c => $"{c}").ToArray();
                }
            }}

            /// <summary>
            /// Not applicable to text.
            /// </summary>
            public override string PathStrokePattern => throw new NotImplementedException("N/A"); // text doesn't have a stroke pattern

            string ISVGElement.ToSVGElement(SVG svg) {

                double xpos   = 0;
                double ypos   = 1.5;
                double width  = 0;

                double linespacing = (LineSpacing) / LineHeight + 2; // font space units

                List<GlyphRef> textbox = new();

                foreach( string line in Lines ) {
                    foreach( char c in line ) {
                        if( !Font.Glyphs.TryGetValue(c, out Font.Glyph? glyph) ) {
                            xpos += Font.WordSpacing; // assume this was a space
                            continue;
                        }

                        textbox.Add( new GlyphRef(
                            glyph, 
                            new Point(xpos, ypos), 
                            ENUMS.COLORS.Lookup(Color)
                        ));

                        xpos += glyph.XMax + (Math.PI/2.0) * glyph.XMin + Font.LetterSpacing; // don't ask why pi/2 is here, it just is
                        width = Math.Max(width, xpos);
                    }

                    ypos += linespacing;
                    xpos = 0;
                }
                ypos -= linespacing; // fixes off-by-1 error

                double horizOffset = HAlign switch {
                    ALIGN.AFTER  => -width,
                    ALIGN.MIDDLE => -width / 2.0,
                    ALIGN.BEFORE => 0,
                    _            => throw new NotImplementedException("Unknown horizontal alignment")
                };

                double vertOffset = VAlign switch {
                    ALIGN.AFTER  => 2.5 - ypos,
                    ALIGN.MIDDLE => 1.0 - ypos / 2.0,
                    ALIGN.BEFORE => -0.5,
                    _            => throw new NotImplementedException("Unknown vertical alignment")
                };

                StringBuilder svgText = new();
                svgText.Append($"<g x='0' y='0' transform='translate({Origin.X}, {Origin.Y}) rotate({-Angle}) scale({LineHeight * WHRatio}, {LineHeight}) skewX({-Inclination}) translate({horizOffset}, {vertOffset})'>");
                foreach( GlyphRef pg in textbox ) {
                    svgText.Append( pg.ToSVGElement(svg) );
                }
                svgText.Append("</g>");

                return svgText.ToString();
            }

        }

    }
}
