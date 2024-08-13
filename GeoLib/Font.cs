using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using static SharpTech.GEOLib.Font.Glyph;
using static SharpTech.GEOLib.RE;
using static SharpTech.GEOLib.Text;

namespace SharpTech {
    public partial class GEOLib {

        internal partial class Font {

            [GeneratedRegex($@"#~HEADER\r?\n.*?\r?\n({INT})\r?\n({INT})\r?\n({INT})\r?\n({INT})\r?\n({INT})\r?\n({DEC})", RegexOptions.Multiline)]
            private static partial Regex HeaderPattern();

            [GeneratedRegex($@"#~2\r?\n(.)\r?\n{INT}\r?\n((?:#~2\.1\r?\n{INT}\r?\n(?:.(?: {INT} {INT})?\r?\n)+\|~\r?\n)+)", RegexOptions.Multiline | RegexOptions.Singleline)]
            private static partial Regex GlyphPattern();

            [GeneratedRegex($@"#~2\.1\r?\n{INT}\r?\n((?:[MD] {INT} {INT}\r?\n)+)(C)?", RegexOptions.Multiline | RegexOptions.Singleline)]
            private static partial Regex ContourPattern();

            [GeneratedRegex($@"([MD]) ({INT}) ({INT})", RegexOptions.Multiline)]
            private static partial Regex InstructionPattern();

            Dictionary<char, Glyph> Glyphs = [];

            double MagicNumber { get; }
            double Baseline    { get; }


            public Font( string resource ) {

                var ctx  = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource) ?? throw new ArgumentException($"Font '{resource}' not found");
                var data = new StreamReader(ctx).ReadToEnd();

                var headerBlock = HeaderPattern().MatchOrElse(data, "Bad font header");

                MagicNumber = double.Parse(headerBlock.Groups[6].Value);
                Baseline    = double.Parse(headerBlock.Groups[5].Value);

                foreach( Match glyphBlock in GlyphPattern().Matches(data) ) {
                    string alias = HttpUtility.HtmlEncode( glyphBlock.Groups[1].Value );
                    Glyphs.Add(
                        glyphBlock.Groups[1].Value[0],
                        new Glyph(
                            alias,
                            glyphBlock.Groups[2].Value,
                            this
                        )
                    );
                }

            }

            internal class Glyph : ISVGElement {

                private readonly string Instructions;
                private readonly string Alias;

                internal Glyph( string alias, string data, Font parent ) {

                    Alias = alias;

                    List<Instruction> instructions = new List<Instruction>();

                    foreach( Match contourBlock in ContourPattern().Matches(data) ) {

                        foreach( Match instructionBlock in InstructionPattern().Matches(contourBlock.Groups[1].Value) ) {
                            instructions.Add( new Instruction(
                                instructionBlock.Groups[1].Value[0] == 'M' ? 'M' : 'L',
                                double.Parse(instructionBlock.Groups[2].Value),
                                (double.Parse(instructionBlock.Groups[3].Value) - parent.Baseline) / parent.MagicNumber // don't ask
                            ));
                        }

                        if( contourBlock.Groups[2].Success ) {
                            var first = instructions[0];
                            instructions.Add( new Instruction(
                                'L',
                                first.X,
                                first.Y
                            ));
                        }

                    }

                    Instructions = string.Join( "", instructions.Select( i => $"{i.OP}{i.X},{i.Y}" ) );

                }

                internal class Instruction(char op, double x, double y) {
                    public readonly char   OP = op;
                    public readonly double X  = x;
                    public readonly double Y  = y;
                }

                public string ToSVGElement(SVG svg) {
                    return $"<refs><path d='{Instructions}' id='{Alias}'/></refs>";
                }

            }

        }

    }
}
