using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Fasteroid {
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
            float PathStrokeWidth { get => PathStrokePattern == null ? 1 : 2; }

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

            public float Width;
            public float Height;

            public readonly List<ISVGElement> Children = new();

            private int idAcc = 0;

            public SVG(float width, float height) {
                Width = width;
                Height = height;
            }

            public override string ToString() {
                StringBuilder svg = new();
                svg.Append($@"<svg xmlns=""http://www.w3.org/2000/svg"" width=""100%"" viewBox=""0 0 {Width} {-Height}"">");
                svg.Append("<style> * { vector-effect: non-scaling-stroke; } </style>");
                svg.Append($@"<g transform=""translate(0, {-Height})"">");
                foreach (var child in Children) {
                    var childSVG = child.ToSVGElement(this);
                    if(childSVG != null) svg.Append(childSVG);
                }
                svg.Append("</g>");
                svg.Append("</svg>");
                return svg.ToString();
            }


            // Thankfully order doesn't matter in svg, so we can just let the developer do what they want
            // with this instead of having to maintain some horrible dependency graph of href'able elements
            public int AllocateUniqueID() {
                return idAcc++;
            }

        }
    }
}
