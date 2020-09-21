using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Skybot.Web.Wiki
{
    public static class WikiScriptCommands
    {
        public const string CommandCharacter = "@";


        public const string FontStyle = "font-style";
        public const string FontColor = "font-color";
        public const string FontSize = "font-size";
        public const string FontBig = "font-big";
        public const string FontUnderlined = "font-underline";
        public const string FontCrossedOut = "font-crossed";

        public const string Grid = "grid";
        public const string GridItem = "grid-item";


        public const string LoadFile = "loadfile";

        public const string CurrentTime = "time";
        public const string CurrentDate = "date";
        public const string CurrentDateTime = "datetime";
        public const string Paragraph = "p";
        public const string Div = "div";
        public const string Center = "center";

        public const string Image = "img";
        public const string Link = "url";

        public const string EscapeCharacter = "\\";
        public static readonly Dictionary<string, string> EscapeCharacters = new Dictionary<string, string>()
        {
            { EscapeCharacter + "@", "@" }, //@
            { EscapeCharacter + "t", "\t" }, //tab
            { EscapeCharacter + "n", "<br>\n" }, //line break
            { EscapeCharacter + "\\", "\\"}, // \
            { EscapeCharacter + "ls", "————————————————————————————————" },
            { EscapeCharacter + "ic", SkyBot.Resources.InvisibleCharacter },
            { EscapeCharacter + "br", "<br>\n" }, //line break
        };

        public const string AreaStart = "-st";
        public const string AreaEnd = "-ed";

        public const string CommandTerminator = ";";


        #region command functions
        public static string OnCenterStart(string line, List<string> split)
        {
            return $"<center>{line}";
        }

        public static string OnCenterEnd(string line, List<string> split)
        {
            return $"{line}</center>";
        }

        public static string OnCenterSingle(string line, List<string> split)
        {
            return $"<center>{line}</center>";
        }


        public static string OnDivStart(string line, List<string> split)
        {
            return $"<div>{line}";
        }

        public static string OnDivEnd(string line, List<string> split)
        {
            return $"{line}</div>";
        }

        public static string OnDivSingle(string line, List<string> split)
        {
            return $"<div>{line}</div>";
        }


        public static string OnParagraphStart(string line, List<string> split)
        {
            return $"<p>{line}";
        }

        public static string OnParagraphEnd(string line, List<string> split)
        {
            return $"{line}</p>";
        }

        public static string OnParagraphSingle(string line, List<string> split)
        {
            return $"<p>{line}</p>";
        }


        public static string OnLoadFile(string line, List<string> split)
        {
            if (!File.Exists(line))
                throw new FileNotFoundException("Could not find file", line);

            return File.ReadAllText(line);
        }

        public static string OnCurrentTime(string line, List<string> split)
        {
            string format = "HH:MM:SS:MSMS";

            if (!string.IsNullOrEmpty(line))
                format = line;

            return format.Replace("HH", DateTime.UtcNow.Hour.ToString())
                         .Replace("MM", DateTime.UtcNow.Minute.ToString())
                         .Replace("SS", DateTime.UtcNow.Second.ToString())
                         .Replace("MSMS", DateTime.UtcNow.Millisecond.ToString());
        }

        public static string OnCurrentDate(string line, List<string> split)
        {
            string format = "YYYY:MM:DD";

            if (!string.IsNullOrEmpty(line))
                format = line;

            return format.Replace("YYYY", DateTime.UtcNow.Year.ToString())
                         .Replace("MM", DateTime.UtcNow.Month.ToString())
                         .Replace("DD", DateTime.UtcNow.Day.ToString());
        }

        public static string OnCurrentDateTime(string line, List<string> split)
        {
            string formatYear = "YYYY:MM:DD";
            string formatHour = "HH:MM:SS:MSMS";

            if (!string.IsNullOrEmpty(line))
            {
                int index = line.IndexOf(' ');

                if (index == -1)
                    formatYear = line;
                else
                {
                    formatYear = line.Substring(0, index);
                    formatHour = line.Remove(0, index + 1);
                }
            }

            return $"{OnCurrentDate(formatYear, null)} {OnCurrentTime(formatHour, null)}";
        }

        public static string OnLink(string line, List<string> split)
        {
            if (split.Count == 1)
                return $"<a href=\"{line}\">{line}</a>";

            return $"<a href=\"{split[0]}\">{line.Remove(split[0].Length + 1)}</a>";
        }

        public static string OnLinkStart(string line, List<string> split)
        {
            if (split.Count == 1)
                return $"<a href=\"{line}\">{line}";

            return $"<a href=\"{split[0]}\">{line.Remove(split[0].Length + 1)}";
        }

        public static string OnImage(string line, List<string> split)
        {
            return $"<img src=\"{line}\" alt=\"{line}\">";
        }

        public static string OnLinkEnd(string line, List<string> split)
        {
            return $"{line}</a>";
        }
        #endregion
    }
}
