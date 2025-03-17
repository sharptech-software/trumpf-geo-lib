using System.Text;
using System.Text.RegularExpressions;
using Fasteroid;

namespace SharpTech {

    
    public partial class GEOLib {

        /// <summary>
        /// A compound SVG path.
        /// </summary>
        public class Contour(IEnumerable< IEnumerable<IStroke> > Strokes): ISVGElement {

            /// <summary>
            /// A set of sets of strokes.
            /// Each subset is a closed path.
            /// </summary>
            public IEnumerable< IEnumerable<IStroke> > Strokes { get; } = Strokes;

            string ISVGElement.ToSVGElement(SVG parent) {
                var path = new StringBuilder();

                foreach( var subStrokes in Strokes ) {
                    path.Append( subStrokes.First().StrokeStart + String.Join(' ', subStrokes.Select( sub => sub.StrokeBody )) + "Z" );
                }

                return CreatePath( path.ToString(), "none", "#777", 0, null );
            }
        }

    }
}
