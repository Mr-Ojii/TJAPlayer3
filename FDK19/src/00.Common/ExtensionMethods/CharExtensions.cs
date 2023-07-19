using System;
using System.Linq;

namespace FDK.ExtensionMethods;

public static class CharExtensions
{
    public static bool ToBool(this char c)
    {
        return ( c != '0' );
    }
}
