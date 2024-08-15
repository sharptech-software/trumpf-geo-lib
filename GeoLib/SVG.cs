using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SharpTech {
    public partial class GEOLib {

        /// <summary>
        /// Represents an SVG element.
        /// </summary>
        public interface ISVGElement {

            /// <summary>
            /// Given an <see cref="SVG"/> parent container, returns this child as a string.<br/>
            /// The parent is passed so children can add shared references as needed.
            /// </summary>
            /// <returns>An svg element</returns>
            string? ToSVGElement(SVG parent) => null;

        }

        /// <summary>
        /// Represents an SVG path element.
        /// </summary>
        public interface ISVGPath : ISVGElement {

            /// <summary>
            /// The 'd' attribute of the path.
            /// <code>
            /// &lt;path d="<see cref="PathInstructions">this</see>" .../&gt;
            /// </code>
            /// </summary>
            string PathInstructions { get; }

            /// <summary>
            /// Stroke color, as HTML color string.
            /// </summary>
            string PathColor { get => "black"; }

            /// <summary>
            /// The stroke-dasharray attribute of the path, or null if nothing special.
            /// <code>
            /// &lt;path stroke-dasharray="<see cref="PathInstructions">this</see>" .../&gt;
            /// </code>
            /// </summary>
            string? PathStrokePattern { get => null; }

            /// <summary>
            /// Stroke width.
            /// </summary>
            double PathStrokeWidth { get => PathStrokePattern == null ? 1 : 2; }

            string ISVGElement.ToSVGElement(SVG parent)
            {
                StringBuilder svg = new();
                svg.Append($@"<path d=""{PathInstructions}"" fill=""none"" stroke=""{PathColor}"" stroke-width=""{PathStrokeWidth}"" stroke-linecap=""round""");
                if (PathStrokePattern != null)
                {
                    svg.Append($@" stroke-dasharray=""{PathStrokePattern}""");
                }
                svg.Append("/>");
                return svg.ToString();
            }

        }

        /// <summary>
        /// A container with a width and height containing <see cref="ISVGElement"/>s.
        /// </summary>
        public class SVG {

            internal class PathInstruction(char op, double x, double y) {
                public readonly char   OP = op;
                public readonly double X  = x;
                public readonly double Y  = y;
                public override string ToString() => $"{OP} {X:F5}, {Y:F5} ";
            }

            /// <summary>
            /// SVG width.
            /// </summary>
            public double Width;

            /// <summary>
            /// SVG height.
            /// </summary>
            public double Height;

            /// <summary>
            /// The children of this SVG.
            /// </summary>
            public readonly List<ISVGElement> Children = new();

            private int idAcc = 0;
            private HashSet<string> globals = new();

            internal SVG(double width, double height) {
                Width = width;
                Height = height;
            }

            /// <summary>
            /// Returns this SVG as a string.
            /// </summary>
            public override string ToString() {
                StringBuilder svg = new();
                svg.Append($@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""100%"" viewBox=""0 0 {Width} {Height}"">");
                svg.Append("<style> * { vector-effect: non-scaling-stroke } .text { fill: none; stroke-width: 1 } </style>");
                svg.Append($@"<g transform=""translate(0, {Height})"">");
                for( int i = 0; i < Children.Count; i++) { // can't use enumeration because we might add more children... stupid C#
                    var child = Children[i];
                    var childSVG = child.ToSVGElement(this);
                    if(childSVG != null) svg.Append(childSVG);
                }
                svg.Append("</g>");
                svg.Append("</svg>");
                return svg.ToString();
            }

            /// <summary>
            /// Allocates a guaranteed unique id within this SVG.
            /// </summary>
            /// <returns></returns>
            public int AllocateUniqueID() {
                return idAcc++;
            }

            /// <summary>
            /// Helpful to ensure you create only one of something.
            /// </summary>
            /// <param name="feature"></param>
            /// <returns>True if new, false if it exists already</returns>
            public bool AllocateSharedFeature(string feature) {
                if( globals.Contains(feature) ) return false;
                globals.Add(feature);
                return true;
            }

        }

    }
}
