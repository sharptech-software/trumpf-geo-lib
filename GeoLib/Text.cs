using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {

        public static partial class ENUMS {
            public static class TEXT {

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
                    public static string Vertical(int alignment) {
                        return alignment switch {
                            AFTER  => "hanging",
                            MIDDLE => "middle",
                            BEFORE => "baseline",
                            _      => throw new ArgumentException("Tried to look up nonexistant alignment type")
                        };
                    }
                    public static string Horizontal(int alignment) {
                        return alignment switch {
                            AFTER  => "end",
                            MIDDLE => "middle",
                            BEFORE => "start",
                            _      => throw new ArgumentException("Tried to look up nonexistant alignment type")
                        };
                    }
                }
            }
        }

        public partial class Text : Entity, ISVGElement {

            [GeneratedRegex($@"^({RE.DEC}) ({RE.DEC}) ({RE.DEC})", RegexOptions.Singleline)]
            private static partial Regex Layout1Pattern();

            [GeneratedRegex($@"^({RE.INT}) ({RE.INT}) ({RE.INT})", RegexOptions.Singleline)]
            private static partial Regex Layout2Pattern();

            [GeneratedRegex($@"^({RE.DEC}) ({RE.DEC})", RegexOptions.Singleline)]
            private static partial Regex AnglePattern();

            protected override Attribute? GetAttFromData(ref ReadOnlySpan<char> entdata) => null;
            public override Attribute? Att { get; } // we'll set this up here instead

            public Point  Origin     { get; }
            public float  LineHeight { get; }
            public float  WHRatio    { get; }
            public float  Angle      { get; }
            public string InnerText  { get; }

            private readonly int _Alignment;

            public int VAlign => (_Alignment & 0b111);
            public int HAlign => (_Alignment >> 3);

            public string Font => ENUMS.TEXT.FONTS.Lookup(Stroke); // this is not a mistake, the "stroke" field is used for font type on text entities

            internal Text(ReadOnlySpan<char> textblock, Drawing parent) : base(ref textblock, parent, ENUMS.ENTITY.CIRCLE) {

                textblock = textblock.TakeLines(1, out string origin)
                                     .TakeLines(1, out string layout1)
                                     .TakeLines(1, out string angle)
                                     .TakeLines(1, out string layout2);

                Origin = parent.LookupPoint( int.Parse(origin) );

                var layout1Match = Layout1Pattern().MatchOrElse(layout1, $"Malformed text layout 1 section: {layout1}");
                var angleMatch   = AnglePattern().MatchOrElse(angle, $"Malformed text angle section: {angle}");
                var layout2Match = Layout2Pattern().MatchOrElse(layout2, $"Malformed text layout 2 section: {layout2}");

                LineHeight = float.Parse(layout1Match.Groups[1].Value);
                WHRatio    = float.Parse(layout1Match.Groups[2].Value);

                Angle      = float.Parse(angleMatch.Groups[2].Value);

                _Alignment = int.Parse(layout2Match.Groups[1].Value);

                int numOfLines = int.Parse(layout2Match.Groups[3].Value);

                textblock = textblock.TakeLines(numOfLines, out string text);
                InnerText = text;

                try {
                    Att = base.GetAttFromData(ref textblock);
                }
                catch {
                    Att = null;
                }
            }

            internal string Rows { get { 
                var lines = InnerText.Split('\n');
                return lines.Select(
                    (line, idx) => {
                        return VAlign switch {
                            ENUMS.TEXT.ALIGN.AFTER  => $"<tspan x='0' y='{idx + 1}em'>{line}</tspan>",
                            ENUMS.TEXT.ALIGN.MIDDLE => $"<tspan x='0' y='{idx - lines.Length/2 + 0.5}em'>{line}</tspan>",
                            ENUMS.TEXT.ALIGN.BEFORE => $"<tspan x='0' y='{idx - lines.Length + 1}em'>{line}</tspan>",
                            _ => throw new ArgumentException("Tried to look up nonexistant alignment type")
                        };
                    }
                )
                .Aggregate( (a, b) => a + b );
            } }


            // svg interface
            public override string PathStrokePattern => throw new NotImplementedException("N/A"); // text doesn't have a stroke pattern

            string ISVGElement.ToSVGElement(SVG svg) {

                if( svg.AllocateSharedFeature($"font_{Stroke}") ) { // make sure we only add the font once, and only if we need it
                    svg.Children.Add( new SVGBase64Font(Stroke) );
                }
                return 
                    
$@"<text  
font-family='{Font}' 
font-size='{LineHeight}px' 
stroke='{PathColor}'
transform='translate({Origin.X} {Origin.Y}) rotate({-Angle})' 
text-anchor='{ENUMS.TEXT.ALIGN.Horizontal(HAlign)}'
>
{Rows}
</text>
<circle r=""0.02"" cx=""{Origin.X}"" cy=""{Origin.Y}"" fill=""red"" />
";

            }

        }


    }
}
