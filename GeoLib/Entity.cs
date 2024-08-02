using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {

        public partial class Entity {

            // "entblock" refers to an entity block with the first line (its type) removed
            // "entdata"  refers to everything that follows the standard properties for all entities

            [GeneratedRegex($@"^({RE.INT}) ({RE.INT})$", RegexOptions.Singleline)]
            protected static partial Regex AppearancePattern();

            public virtual string Type   { get; }
            public virtual int    Color  { get; }
            public virtual int    Stroke { get; }

            protected virtual bool ParseAttByDefault => true;
            public    virtual Attribute? Att { get; }

            public readonly Drawing Parent;

            public Entity( ReadOnlySpan<char> entblock, Drawing parent, string type ): this(entblock, parent, type, out ReadOnlySpan<char> _) { }

            public Entity( ReadOnlySpan<char> entblock, Drawing parent, string type, out ReadOnlySpan<char> entdata) {
                Parent  = parent;
                entdata = entblock.TakeLines(1, out string appearance);

                Type = type;

                var appearanceMatch = AppearancePattern().MatchOrElse(appearance, $"Malformed entity: {appearance}");
                Color  = int.Parse(appearanceMatch.Groups[1].Value);
                Stroke = int.Parse(appearanceMatch.Groups[2].Value);

                if( ParseAttByDefault ) {
                    var attRemoved = entdata.TakeLinesFromEnd(1, out string strAttRef)
                                            .TakeLinesFromEnd(1, out string strIdk); // not sure what this, but it's always a single int

                    if( int.TryParse(strAttRef, out int attRef) && int.TryParse(strIdk, out int _) ) {
                        Att = parent.Atts.GetOrElse(attRef, $"Attribute {attRef} not found");
                        entdata = attRemoved;
                    }
                }
            }

            // svg interface
            public virtual string  PathColor => CONSTANTS.COLORS.Lookup(Color);
            public virtual string? PathDashPattern => CONSTANTS.STROKES.Lookup(Stroke);

            public static Entity FromBlock( string block, Drawing parent ) {
                var entblock = block.TakeLines(1, out string type);
                switch( type ) {
                    case CONSTANTS.ENTITY.LINE:
                        return new Line(entblock, parent);
                    case CONSTANTS.ENTITY.CIRCLE:
                        return new Circle(entblock, parent);
                    case CONSTANTS.ENTITY.ARC:
                        return new Arc(entblock, parent);
                    default:
                        return new Entity(entblock, parent, type);
                }
            }
        }

    }
}
