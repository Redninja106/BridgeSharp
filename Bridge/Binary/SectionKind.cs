using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge.Binary;

public enum SectionKind : byte
{
    Header,
    Code,
    Data,
}
