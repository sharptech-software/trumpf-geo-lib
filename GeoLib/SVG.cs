using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SharpTech {
    public partial class GEOLib {

        public interface ISVGElement {
            /// <summary>
            /// Provides the parent SVG so the developer can allocate unique IDs for hrefs.
            /// </summary>
            /// <returns>svg code</returns>
            string? ToSVGElement(SVG parent) => null;
        }

        public interface ISVGPath : ISVGElement {
            string PathInstructions { get; }
            string PathColor { get => "black"; }
            string? PathStrokePattern { get => null; }
            double PathStrokeWidth { get => PathStrokePattern == null ? 1 : 2; }

            // Default implementation of ToSVGElement for ISVGPath
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

        public class SVG {

            internal class PathInstruction(char op, double x, double y) {
                public readonly char   OP = op;
                public readonly double X  = x;
                public readonly double Y  = y;
                public override string ToString() => $"{OP}{X},{Y}";
            }

            public double Width;
            public double Height;

            public readonly List<ISVGElement> Children = new();

            private int idAcc = 0;
            private HashSet<string> globals = new();

            public SVG(double width, double height) {
                Width = width;
                Height = height;
            }

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
            /// <param name="id"></param>
            /// <returns>false if it already exists, true if it was created</returns>
            public bool AllocateSharedFeature(string feature) {
                if( globals.Contains(feature) ) return false;
                globals.Add(feature);
                return true;
            }

        }

    }
}
