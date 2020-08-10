using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable CA1724 //The type name Extensions conflicts in whole or in part with the namespace name 'Microsoft.Extensions'. Change either name to eliminate the conflict
public static class Extensions
#pragma warning restore CA1724 //The type name Extensions conflicts in whole or in part with the namespace name 'Microsoft.Extensions'. Change either name to eliminate the conflict
{
    public static bool TryParseEnum<T>(this string value, out T outp) where T : struct, Enum
    {
        if (long.TryParse(value, out long lres) && Enum.IsDefined(typeof(T), lres))
        {
            outp = (T)(object)lres;
            return true;
        }
        else if (Enum.TryParse<T>(value, out T res))
        {
            outp = res;
            return true;
        }
        else
        {
            outp = default;
            return false;
        }
    }
}
