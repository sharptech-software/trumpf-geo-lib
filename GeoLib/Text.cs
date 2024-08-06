using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {

        public partial class Text : Entity {

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

            private readonly int _Justification;
            public (int, int) Justification => ((_Justification >> 3) - 1, (_Justification & 7) - 2);

            internal Text(ReadOnlySpan<char> textblock, Drawing parent) : base(ref textblock, parent, CONSTANTS.ENTITY.CIRCLE) {

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

                _Justification = int.Parse(layout2Match.Groups[1].Value);

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
        }

    }
}
