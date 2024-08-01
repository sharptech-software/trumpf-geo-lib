using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fasteroid {
    public partial class GEOLib {

        public abstract class Attribute {
            public abstract Type Type { get; }
        }
    }
}
