using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bridge;
public readonly struct DataEntry
{
    public readonly int value;

    public DataEntry(int value)
    {
        this.value = value;
    }

    public static implicit operator int(DataEntry entry) => entry.value;
    public static implicit operator DataEntry(int value) => new(value);
}
