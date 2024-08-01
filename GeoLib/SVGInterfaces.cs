using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fasteroid {
    public partial class GEOLib {
        public interface ISVGPath {

            string  PathInstructions { get; }
            virtual string  PathColor         => "black";
            virtual string? PathStrokePattern => null;
            virtual string  PathStrokeWidth   => PathStrokePattern == null ? "1" : "2";

            public string ToPath() {
                StringBuilder svg = new();
                svg.Append($@"<path d=""{PathInstructions}"" fill=""none"" stroke=""{PathColor}"" stroke-width=""{PathStrokeWidth}"" stroke-linecap=""round""");
                if (PathStrokePattern != null) {
                    svg.Append($@" stroke-dasharray=""{PathStrokePattern}""");
                }
                svg.Append("/>");
                return svg.ToString();
            }

        }
    }
}
