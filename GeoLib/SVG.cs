using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SharpTech {
    public partial class GEOLib {

        internal static string CreatePath(string Path, string? Class, string? FillColor, string? StrokeColor, double StrokeWidth, string? StrokePattern) {
            StringBuilder svg = new();

            svg.Append($@"<path d=""{Path}"" stroke-width=""{StrokeWidth}"" stroke-linecap=""round""");
                if( Class != null ) svg.Append($@" class=""{Class}""");
                if( StrokePattern != null ) svg.Append($@" stroke-dasharray=""{StrokePattern}""");
                if( StrokeColor != null ) svg.Append($@" stroke=""{StrokeColor}""");
                if( FillColor != null ) svg.Append($@" fill=""{FillColor}""");
            svg.Append("/>");

            return svg.ToString();
        }

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
        public interface IStroke : ISVGElement {

            /// <summary>
            /// The 'M' part of the path, if it were the first command.
            /// </summary>
            string StrokeStart { get; }

            /// <summary>
            /// Everything after the 'M' part of the path.
            /// </summary>
            string StrokeBody { get; }

            /// <summary>
            /// Stroke color, as HTML color string.
            /// </summary>
            string? StrokeColor { get => null; }

            /// <summary>
            /// The stroke-dasharray attribute of the path, or null if nothing special.
            /// <code>
            /// &lt;path stroke-dasharray="<see cref="StrokeBody">this</see>" .../&gt;
            /// </code>
            /// </summary>
            string? StrokePattern { get => null; }

            /// <summary>
            /// Stroke width.
            /// </summary>
            double StrokeWidth { get => StrokePattern == null ? 1 : 2; }

            string ISVGElement.ToSVGElement(SVG parent)
            {
                return CreatePath(
                    Path:          StrokeStart + StrokeBody,
                    Class:         null,
                    StrokeColor:   StrokeColor ?? parent.DefaultStrokeColor,
                    FillColor:     "none",
                    StrokeWidth:   StrokeWidth,
                    StrokePattern: StrokePattern
                );
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

            /// <inheritdoc cref="Drawing.Responsive"/>
            public bool Responsive = true;

            /// <inheritdoc cref="Drawing.StrokeColor"/>
            public string? DefaultStrokeColor = null;

            /// <summary>
            /// The children of this SVG.
            /// </summary>
            public readonly List<ISVGElement> Children = new();

            private int idAcc = 0;
            private HashSet<string> globals = new();

            internal SVG(double width, double height, string? defaultStrokeColor, bool responsive) {
                Width = width;
                Height = height;
                DefaultStrokeColor = defaultStrokeColor;
                Responsive = responsive;
            }

            /// <summary>
            /// Returns this SVG as a string.
            /// </summary>
            public override string ToString() {
                var size = Responsive ? $@"width=""100%""" : $@"width=""{Width}"" height=""{Height}""";

                StringBuilder svg = new();
                svg.Append($@"<svg xmlns=""http://www.w3.org/2000/svg"" {size} viewBox=""0 0 {Width} {Height}"">");
                svg.Append($"<style> * {{ vector-effect: non-scaling-stroke; fill-rule: evenodd; }} .text {{ fill: none; stroke-width: 1; }} </style>");
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
