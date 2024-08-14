using System.Text;
using System.Text.RegularExpressions;
using Fasteroid;

namespace SharpTech {
    public partial class GEOLib {

        public partial class Text : Entity, ISVGElement {

            internal class GlyphRef : ISVGElement {
                public readonly Font.Glyph Glyph;
                public readonly Point      Position;
                public readonly string     Color;

                public GlyphRef(Font.Glyph glyph, Point position, string color) {
                    Color    = color;
                    Glyph    = glyph;
                    Position = position;
                }

                public string ToSVGElement(SVG parent) {
                    if( !parent.AllocateSharedFeature(Glyph.Name) ) { // ensure reference is available
                        parent.Children.Add(Glyph);
                    }
                    return $@"<use href='#{Glyph.Name}' x='{Position.X}' y='{Position.Y}' stroke='{Color}'/>";
                }
            }

            public static class ALIGN {
                public const int AFTER  = 0b100;
                public const int MIDDLE = 0b010;
                public const int BEFORE = 0b001;
            }

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


            protected override Attribute? GetAttFromData(ref ReadOnlySpan<char> entdata) => null;
            public override Attribute? Att { get; } // we'll set this up here instead

            public Point  Origin      { get; }


            public double  LineHeight  { get; }
            public double  WHRatio     { get; }
            public double  Inclination { get; }


            public double  LineSpacing { get; }
            public double  Angle       { get; }

            public int VAlign   { get; }  public int HAlign { get; }
            public int WriteDir { get; }

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
                    Att = base.GetAttFromData(ref textblock);
                }
                catch {
                    Att = null;
                }
            }

            public string[] Lines { get {
                if( WriteDir == DIRECTION.LEFT ) {
                    return InnerText.Split('\n');
                }
                else {
                    return InnerText.Select(c => $"{c}").ToArray();
                }
            }}

            // svg interface
            public override string PathStrokePattern => throw new NotImplementedException("N/A"); // text doesn't have a stroke pattern

            string ISVGElement.ToSVGElement(SVG svg) {

                double xpos  = 0;
                double ypos  = 0;

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

                        xpos += glyph.XMax - (Math.PI/2) * glyph.XMin + Font.LetterSpacing; // don't ask why pi/2 is here, it just is
                    }
                    ypos += 1; // todo: line height; for now we'll just use 1
                    xpos = 0;
                }

                StringBuilder svgText = new();
                svgText.Append($"<g transform='translate({Origin.X}, {Origin.Y})'>");
                foreach( GlyphRef pg in textbox ) {
                    svgText.Append( pg.ToSVGElement(svg) );
                }
                svgText.Append("</g>");

                return svgText.ToString();
            }

        }

    }
}
