using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {

        public partial class Text : Entity, ISVGElement {

            public static class FONTS {
                public const int BOLD    = 131;
                public const int ISOPROP = 130;
                public const int ISODIM  = 9;
                public const int ISO     = 1;

                public static string Lookup(int font) => font switch {
                    BOLD    => "BOLD",
                    ISOPROP => "ISOPROP",
                    ISODIM  => "iso_dim",
                    ISO     => "ISO",
                    _       => throw new ArgumentException("Tried to look up nonexistant font type")
                };
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


            // weird magic numbers from TRUMPF; they make things work
            internal const double WH_SCALAR = 1.2;
            internal const double LH_SCALAR = 9.0;
            internal const double LH_OFFSET = 0.8;


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

            
            public string InnerText   { get; }

            public string Font => FONTS.Lookup(Stroke); // this is not a mistake, the "stroke" field is used for font type on text entities

            internal Text(ReadOnlySpan<char> textblock, Drawing parent) : base(ref textblock, parent, ENUMS.ENTITY.CIRCLE) {

                textblock = textblock.TakeLines(1, out string origin)
                                     .TakeLines(1, out string part1)
                                     .TakeLines(1, out string part2)
                                     .TakeLines(1, out string part3);

                Origin = parent.LookupPoint( int.Parse(origin) );

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
                        InnerText = text;
                }

                try {
                    Att = base.GetAttFromData(ref textblock);
                }
                catch {
                    Att = null;
                }
            }

            internal string Rows { get { 

                double height = (1 + LineSpacing * LH_SCALAR + LH_OFFSET);

                string[] lines = WriteDir switch {
                    DIRECTION.LEFT => InnerText.Split('\n'),
                    DIRECTION.DOWN => InnerText.Select(c => c.ToString()).ToArray(),
                    _ => throw new ArgumentException("Tried to look up nonexistant write direction")
                };

                double firstY = VAlign switch {
                    ALIGN.AFTER  => 1,
                    ALIGN.MIDDLE => -((lines.Length - 1)/2.0 * height) + 0.5,
                    ALIGN.BEFORE => -(lines.Length - 1) * height,
                    _ => throw new ArgumentException("Tried to look up nonexistant alignment type")
                };

                return lines.Select(
                    (line, idx) => {
                        return idx == 0 ?
                            $"<tspan x='0' y='{firstY}em'>{ line }</tspan>" :
                            $"<tspan x='0' dy='{height}em'>{ line }</tspan>";
                    }
                )
                .Aggregate( (a, b) => a + b );
            } }


            // svg interface
            public override string PathStrokePattern => throw new NotImplementedException("N/A"); // text doesn't have a stroke pattern

            string ISVGElement.ToSVGElement(SVG svg) {

                var horizontal = HAlign switch {
                    ALIGN.AFTER  => "end",
                    ALIGN.MIDDLE => "middle",
                    ALIGN.BEFORE => "start",
                    _ => throw new ArgumentException("Tried to look up nonexistant alignment type")
                };

                if( svg.AllocateSharedFeature($"font_{Stroke}") ) { // make sure we only add the font once, and only if we need it
                    svg.Children.Add( new SVGBase64Font(Stroke) );
                }
                return 
                    
$@"<text  
font-family='{Font}' 
font-size='{LineHeight}px' 
stroke='{PathColor}'
transform='translate({Origin.X} {Origin.Y}) rotate({-Angle}) scale({WHRatio * WH_SCALAR},1) skewX({-Inclination})' 
text-anchor='{horizontal}'
>
{Rows}
</text>
<circle r=""0.02"" cx=""{Origin.X}"" cy=""{Origin.Y}"" fill=""red"" />
";

            }

        }

        
        public partial class CircularText : Entity, ISVGElement {

        }


    }
}
