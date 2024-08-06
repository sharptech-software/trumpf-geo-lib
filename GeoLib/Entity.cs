using System.Text.RegularExpressions;

namespace Fasteroid {
    public partial class GEOLib {

        public partial class Entity {

            [GeneratedRegex($@"^({RE.INT}) ({RE.INT})$", RegexOptions.Singleline)]
            protected static partial Regex AppearancePattern();

            public readonly Drawing Parent;
            public virtual string     Type   { get; }
            public virtual int        Color  { get; }
            public virtual int        Stroke { get; }
            public virtual Attribute? Att    { get; } // todo: can multiple attributes be attached to an entity?

            /// <summary>
            /// Extracts an attribute from entity data (if it exists) and self-modifies the data to remove the attribute reference.<br/>
            /// Used in the constructor to extract the attribute from the entity block.<br/>
            /// This is virtual so subclasses can override it if data at the end of the block might be mistaken for an attribute.
            /// </summary>
            /// <param name="entdata"></param>
            /// <returns>Attribute if it exists or null</returns>
            protected virtual Attribute? GetAttFromData(ref ReadOnlySpan<char> entdata) {
                var attRemoved = entdata.TakeLinesFromEnd(1, out string strAttRef)
                                        .TakeLinesFromEnd(1, out string strIdk); // not sure what this, but it's always a single int

                if( int.TryParse(strAttRef, out int attRef) && int.TryParse(strIdk, out int _) ) {
                    entdata = attRemoved;
                    return Parent.Atts.GetOrElse(attRef, $"Attribute {attRef} not found");
                }
                return null;
            }

            /// <summary>
            /// If this is false after an entity instantiates, <see cref="FromBlock(string, Drawing)"/> won't return the entity.
            /// </summary>
            protected virtual bool ShouldRender => Att?.Type != CONSTANTS.ATTRIBUTE.TEXT_SLAVE;

            /// <summary>
            /// Base entity constructor; least descriptive.<br/>
            /// Probably shouldn't be used directly.
            /// </summary>
            /// <param name="block">An entity block from the GEO file, which has had its type stripped via <see cref="FromBlock(string, Drawing)"/></param>
            protected Entity( ref ReadOnlySpan<char> block, Drawing parent, string type ) {
                Parent  = parent;
                block   = block.TakeLines(1, out string appearance);

                Type = type;

                var appearanceMatch = AppearancePattern().MatchOrElse(appearance, $"Malformed entity: {appearance}");
                Color  = int.Parse(appearanceMatch.Groups[1].Value);
                Stroke = int.Parse(appearanceMatch.Groups[2].Value);

                Att = GetAttFromData(ref block);
            }

            // svg interface
            public virtual string  PathColor => CONSTANTS.COLORS.Lookup(Color);
            public virtual string? PathDashPattern => CONSTANTS.STROKES.Lookup(Stroke);

            /// <summary>
            /// Creates a drawing entity from a block of entity data.
            /// </summary>
            public static Entity? FromBlock( string block, Drawing parent ) {
                var entblock = block.TakeLines(1, out string type);
                var ent = (type) switch {
                    CONSTANTS.ENTITY.LINE   => new Line(entblock, parent),
                    CONSTANTS.ENTITY.CIRCLE => new Circle(entblock, parent),
                    CONSTANTS.ENTITY.ARC    => new Arc(entblock, parent),
                    _                       => new Entity(ref entblock, parent, type)
                };
                if( ent.ShouldRender ) return ent;
                else return null;
            }
        }

    }
}
