using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Workflows.Wrappers
{
    /// <summary>
    /// Convert wrapper, contains method to help converting objects
    /// </summary>
    public class ConvertWrapper
    {
        internal ConvertWrapper()
        {
            
        }

        /// <summary>
        /// Converts <paramref name="value"/> to <see cref="int"/>
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted value, if failed returns default value: 0</returns>
        public int StrToInt(string value)
        {
            if (int.TryParse(value, out int r))
                return r;

            return 0;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to <see cref="long"/>
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted value, if failed returns default value: 0</returns>
        public long StrToRealLong(string value)
        {
            if (long.TryParse(value, out long r))
                return r;

            return 0L;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to <see cref="ulong"/>
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted value, if failed returns default value: 0</returns>
        public ulong StrToRealUlong(string value)
        {
            if (ulong.TryParse(value, out ulong r))
                return r;

            return 0UL;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to <see cref="double"/>
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted value, if failed returns default value: 0</returns>
        public double StrToDouble(string value)
        {
            if (double.TryParse(value, out double r))
                return r;

            return 0.0;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to <see cref="bool"/>
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted value, if failed returns default value: false</returns>
        public bool StrToBool(string value)
        {
            if (bool.TryParse(value, out bool r))
                return r;

            return false;
        }
    }
}
