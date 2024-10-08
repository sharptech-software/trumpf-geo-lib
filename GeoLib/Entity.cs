﻿using System.Text.RegularExpressions;
using Fasteroid;

namespace SharpTech {
    public partial class GEOLib {

        /// <summary>
        /// A GEO drawing entity.
        /// </summary>
        public partial class Entity : ISVGElement {

            [GeneratedRegex($@"^({RE.INT}) ({RE.INT})$", RegexOptions.Singleline)]
            private static partial Regex AppearancePattern();

            /// <summary>
            /// The drawing this entity belongs to.
            /// </summary>
            public readonly Drawing Parent;

            /// <summary>
            /// See <see cref="ENUMS.ENTITY"/>.
            /// </summary>
            public virtual string Type { get; }

            /// <summary>
            /// See <see cref="ENUMS.COLORS"/>.
            /// </summary>
            public virtual int Color  { get; }

            /// <summary>
            /// See <see cref="ENUMS.STROKES"/>.
            /// </summary>
            public virtual int Stroke { get; }

            /// <summary>
            /// This entity's <see cref="GEOLib.Attribute"/>, if it has one.
            /// </summary>
            /// <remarks>
            /// If multiple attributes is a thing (which I have yet to witness if so), this will only return the first one.<br/>
            /// </remarks>
            public virtual Attribute? Attribute { get; }

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

                if(int.TryParse(strAttRef, out int attRef) && int.TryParse(strIdk, out int _)) {
                    entdata = attRemoved;
                    return Parent.Attributes.GetOrElse(attRef, $"Attribute {attRef} not found"  );
                }
                return null;
            }

            /// <summary>
            /// If this is false after an entity instantiates, <see cref="FromBlock(string, Drawing)"/> won't return the entity.
            /// </summary>
            protected virtual bool ShouldRender => true;

            /// <summary>
            /// Base entity constructor; least descriptive.<br/>
            /// Probably shouldn't be used directly.
            /// </summary>
            /// <param name="block">An entity block from the GEO file, which has had its type stripped via <see cref="FromBlock(string, Drawing)"/></param>
            /// <param name="parent">The drawing this entity belongs to</param>
            /// <param name="type">The type of entity</param>
            protected internal Entity(ref ReadOnlySpan<char> block, Drawing parent, string type) {
                Parent  = parent;
                block   = block.TakeLines(1, out string appearance);

                Type = type;

                var appearanceMatch = AppearancePattern().MatchOrElse(appearance, $"Malformed entity: {appearance}"  );
                Color  = int.Parse(appearanceMatch.Groups[1].Value);
                Stroke = int.Parse(appearanceMatch.Groups[2].Value);

                Attribute = GetAttFromData(ref block);
            }

            /// <inheritdoc cref="ISVGPath.PathColor"/>
            public virtual string  PathColor         => ENUMS.COLORS.Lookup(Color);

            /// <inheritdoc cref="ISVGPath.PathStrokePattern"/>
            public virtual string? PathStrokePattern => ENUMS.STROKES.Lookup(Stroke);

            /// <summary>
            /// Creates a drawing entity from a block of entity data.
            /// </summary>
            internal static Entity FromBlock(string block, Drawing parent) {
                var entblock = block.TakeLines(1, out string type);
                return (type) switch {
                    ENUMS.ENTITY.LINE   => new Line(entblock, parent),
                    ENUMS.ENTITY.CIRCLE => new Circle(entblock, parent),
                    ENUMS.ENTITY.ARC    => new Arc(entblock, parent),
                    ENUMS.ENTITY.TEXT   => new Text(entblock, parent),
                    _                   => new Entity(ref entblock, parent, type)
                };

            }
        }

    }
}
