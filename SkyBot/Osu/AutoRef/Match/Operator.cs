using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Match
{
    /// <summary>
    /// See https://docs.microsoft.com/en-us/dotnet/api/system.math?view=netcore-3.1 for more info
    /// </summary>
    [Flags]
    public enum Operator
    {
        None,
        Plus = 1000,
        Minus,
        Divide,
        Multiply,
        Exponent,
        Modulo,

        BrO,
        BrC,

        Sin = 10000,
        Cos,
        Abs,
        Cbrt,
        Ceiling,
        Exp,
        Floor,
        ILogB,
        Log,
        Sign,
        Sqrt,
        Tan,
        Truncate,

        // 2 inputs
        IEEERemainder = 100000,
        Atan2,
        /// <summary>
        /// Same as <seealso cref="Log"/> but with 2 inputs
        /// </summary>
        LogDouble,
        Max,
        Min,
        MaxMagnitude,
        Round,
        ScaleB,
    }
}
