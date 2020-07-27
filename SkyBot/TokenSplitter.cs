using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot
{
    public static class TokenSplitter
    {
        /// <summary>
        /// Parses a string into multiple string tokens
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="keyStart">Key start symbol</param>
        /// <param name="valueStart">Value start string</param>
        /// <param name="valueEnd">Value end string</param>
        /// <param name="exception">Null if worked</param>
        /// <returns>Dictionary with all possible parsed values</returns>
        public static Dictionary<string, string> Parse(string input, string keyStart, string valueStart, string valueEnd, out Exception exception)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));
            else if (string.IsNullOrEmpty(keyStart))
                throw new ArgumentNullException(nameof(keyStart));
            else if (string.IsNullOrEmpty(valueStart))
                throw new ArgumentNullException(nameof(valueStart));
            else if (string.IsNullOrEmpty(valueEnd))
                throw new ArgumentNullException(nameof(valueEnd));

            Dictionary<string, string> result = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(input))
            {
                exception = new ArgumentNullException(nameof(input), string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.CannotBeNullEmptyException, "Input"));
                return result;
            }

            string current = input;

            int start = -1;

            try
            {

            while((start = current.IndexOf(keyStart, StringComparison.CurrentCultureIgnoreCase)) > -1)
            {
                //Get Key
                current = current.Remove(0, start + keyStart.Length);
                
                int end = current.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);
                string key = current.Substring(0, end);
                current = current.Remove(0, end + 1);

                start = current.IndexOf(valueStart, StringComparison.CurrentCultureIgnoreCase);
                
                if (start == -1)
                    continue;
                
                current = current.Remove(0, start + valueStart.Length);
                end = current.IndexOf(valueEnd, StringComparison.CurrentCultureIgnoreCase);

                if (end == -1)
                {
                    result.Add(key, current);
                    break;
                }

                result.Add(key, current.Substring(0, end));
                current = current.Remove(0, end + valueEnd.Length);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                return result;
            }

            exception = null;
            return result;
        }
    }
}
