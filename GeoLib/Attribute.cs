using Fasteroid;

namespace SharpTech {
    public partial class GEOLib {
        public class Attribute : ISVGElement {
            public int Type { get; }

            /// <summary>
            /// Base attribute constructor; least descriptive.<br/>
            /// Probably shouldn't be used directly.
            /// </summary>
            /// <param name="type"></param>
            protected Attribute(int type) {
                Type = type;
            }

            /// <summary>
            /// Creates an attribute from a block of attribute data.
            /// </summary>
            /// <param name="block"></param>
            /// <returns></returns>
            public static (int, Attribute) FromBlock(string block) {
                var attdata = block.SkipLines(1) // "ATT"
                                   .TakeLines(1, out string strid)
                                   .TakeLines(1, out string strtype);
                return (
                    int.Parse(strid),
                    new Attribute(int.Parse(strtype))
                );
            }
        }
    }
}
