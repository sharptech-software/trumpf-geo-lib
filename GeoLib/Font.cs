using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using static SharpTech.GEOLib.RE;
using static SharpTech.GEOLib.Text;

namespace SharpTech {
    public partial class GEOLib {

        internal static class FONTS {

            public const int BOLD    = 131;
            public const int ISOPROP = 130;
            public const int ISODIM  = 9;
            public const int ISO     = 1;

            public static readonly ImmutableDictionary<int, Font> Cache = new Dictionary<int, Font> {
                { BOLD, new Font("SharpTech.GEOFonts.BOLD.FNT", 9.9, 80) },            // Where do the numbers come from?
                { ISOPROP, new Font("SharpTech.GEOFonts.ISOPROP.FNT", -2.9, 73.5) },   // https://www.desmos.com/calculator/dulmjw0uo4
                { ISODIM, new Font("SharpTech.GEOFonts.iso_dim.fnt", 4.6, 72.2) },     // I think TRUMPF deliberately made these difficult to reverse-engineer
                { ISO, new Font("SharpTech.GEOFonts.ISO.FNT", 4.6, 72.2) }             // If you find a way to derive these numbers, let me know
            }.ToImmutableDictionary();

        }

        internal partial class Font {

            [GeneratedRegex($@"#~HEADER\r?\n.*?\r?\n({INT})\r?\n({INT})\r?\n({INT})\r?\n({INT})\r?\n({INT})\r?\n({DEC})", RegexOptions.Multiline)]
            private static partial Regex HeaderPattern();

            [GeneratedRegex($@"#~2\r?\n(.)\r?\n{INT}\r?\n((?:#~2\.1\r?\n{INT}\r?\n(?:.(?: {INT} {INT})?\r?\n)+\|~\r?\n)+)", RegexOptions.Multiline | RegexOptions.Singleline)]
            private static partial Regex GlyphPattern();

            [GeneratedRegex($@"#~2\.1\r?\n{INT}\r?\n((?:[MD] {INT} {INT}\r?\n)+)(C)?", RegexOptions.Multiline | RegexOptions.Singleline)]
            private static partial Regex ContourPattern();

            [GeneratedRegex($@"([MD]) ({INT}) ({INT})", RegexOptions.Multiline)]
            private static partial Regex InstructionPattern();

            public ImmutableDictionary<char, Glyph> Glyphs;

            public readonly double LetterSpacing;
            public readonly double WordSpacing;

            public readonly string Name;

            internal Font( string resource, double letter, double word ) {

                var ctx  = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource) ?? throw new ArgumentException($"Font '{resource}' not found");
                var data = new StreamReader(ctx).ReadToEnd();

                var headerBlock = HeaderPattern().MatchOrElse(data, "Bad font header");

                var glyphs = new Dictionary<char, Glyph>();

                double headline = double.Parse(headerBlock.Groups[2].Value);
                double baseline = double.Parse(headerBlock.Groups[4].Value);
                double stretch  = double.Parse(headerBlock.Groups[6].Value);

                LetterSpacing = letter / headline; // relative to glyphs with height of 1
                WordSpacing   = word   / headline;
                Name          = String.Join('.', resource.Split('.').TakeLast(2).ToArray());

                Console.WriteLine($"headline: {headline}");
                Console.WriteLine($"baseline: {baseline}");
                Console.WriteLine($"stretch: {stretch}");

                Console.WriteLine($"letter spacing: {LetterSpacing}");
                Console.WriteLine($"word spacing: {WordSpacing}");

                foreach( Match glyphBlock in GlyphPattern().Matches(data) ) {
                    string charname = HttpUtility.HtmlEncode( glyphBlock.Groups[1].Value );
                    glyphs.TryAdd(
                        glyphBlock.Groups[1].Value[0],
                        new Glyph(
                            $"{Name}.{charname}",
                            headline,
                            baseline,
                            stretch,
                            glyphBlock.Groups[2].Value
                        )
                    );
                }

                Glyphs = glyphs.ToImmutableDictionary();

            }

            public Glyph? Get(char c) {
                return Glyphs.GetValueOrDefault(c);
            }

            internal class Glyph : ISVGElement {

                public readonly string Name;
                public readonly ImmutableArray<SVG.PathInstruction> Instructions;

                public readonly double XMax;
                public readonly double XMin;

                internal Glyph( string name, double headline, double baseline, double stretch, string data ) {

                    Name   = name;
                    List<SVG.PathInstruction> instructions = new List<SVG.PathInstruction>();

                    double xmin = double.MaxValue;
                    double xmax = double.MinValue;

                    foreach( Match contourBlock in ContourPattern().Matches(data) ) {
                        foreach( Match instructionBlock in InstructionPattern().Matches(contourBlock.Groups[1].Value) ) {

                            double x = double.Parse(instructionBlock.Groups[2].Value);
                            double y = -double.Parse(instructionBlock.Groups[3].Value); // invert for proper svg coordinates

                            y -= baseline; // move to baseline (inverted)

                            x /= headline; // scale font to single unit
                            y /= headline;

                            y /= stretch; // fix aspect ratio (don't ask)

                            xmin = Math.Min(xmin, x);
                            xmax = Math.Max(xmax, x);

                            instructions.Add( new SVG.PathInstruction(
                                instructionBlock.Groups[1].Value[0] == 'M' ? 'M' : 'L',
                                x, y
                            ));
                        }

                        if( contourBlock.Groups[2].Success ) {
                            var first = instructions[0];
                            instructions.Add( new SVG.PathInstruction(
                                'L',
                                first.X,
                                first.Y
                            ));
                        }
                        Instructions = [..instructions];
                    }

                    XMax = xmax;
                    XMin = xmin;

                    Console.WriteLine($"glyph {name} spans {xmin} -> {xmax}");

                }

                public string ToSVGElement(SVG svg) {
                    StringBuilder instructionCollector = new();
                    foreach( var instruction in Instructions ) {
                        instructionCollector.Append(instruction);
                    }
                    return $"<refs><path d='{instructionCollector}' id='{Name}' class='text'/></refs>";
                }

            }

        }

    }
}
