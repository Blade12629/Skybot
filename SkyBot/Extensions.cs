using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable CA1724 //The type name Extensions conflicts in whole or in part with the namespace name 'Microsoft.Extensions'. Change either name to eliminate the conflict
public static class Extensions
#pragma warning restore CA1724 //The type name Extensions conflicts in whole or in part with the namespace name 'Microsoft.Extensions'. Change either name to eliminate the conflict
{
    public static bool TryParseEnum<T>(this string value, out T outp) where T : struct, Enum
    {
        if (int.TryParse(value, out int lres) && Enum.IsDefined(typeof(T), lres))
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

    public static int GetLineNumber(this Exception ex)
    {
        const string lineSearch = ":line ";

        if (ex == null || string.IsNullOrEmpty(ex.StackTrace))
            return -1;

        int indexStart = ex.StackTrace.IndexOf(lineSearch, StringComparison.CurrentCultureIgnoreCase);

        if (indexStart == -1)
            return -1;

        string stack = ex.StackTrace.Remove(0, indexStart + lineSearch.Length);

        int indexEnd = stack.IndexOf("\r\n", StringComparison.CurrentCultureIgnoreCase);

        if (indexEnd > 0)
            stack = stack.Substring(0, indexEnd);

        stack = stack.Trim(' ');

        if (int.TryParse(stack, out int lineNumber))
            return lineNumber;

        return -1;
    }
}
