using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot
{
    public static class Extensions
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
}
