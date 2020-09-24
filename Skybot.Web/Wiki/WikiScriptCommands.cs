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

        public const string LoadString = "loadstring";

        public const string Paragraph = "p";
        public const string Div = "div";
        public const string Center = "center";
        public const string Header = "header";
        public const string Body = "body";
        public const string Footer = "footer";

        public const string Image = "img";
        public const string Link = "url";
        public const string CSS = "css";
        public const string CurrentTime = "time";
        public const string CurrentDate = "date";
        public const string CurrentDateTime = "datetime";

        public const string CSCode = "cs";

        public const string EscapeCharacter = "\\";
        public static readonly Dictionary<string, string> EscapeCharacters = new Dictionary<string, string>()
        {
            { EscapeCharacter + "n", "<br>\n" }, //line break
            { EscapeCharacter + "br", "<br>\n" }, //line break
            { EscapeCharacter + "ls", "————————————————————————————————" },
            { EscapeCharacter + "ic", SkyBot.Resources.InvisibleCharacter },

            //ASCII chars
            { EscapeCharacter + "t", "&emsp\\," }, //tab
            { EscapeCharacter + "<", "&#60\\," }, //<
            { EscapeCharacter + ">", "&#62\\," }, //>
            { EscapeCharacter + "@", "&#64\\," } //@
        };

        public static readonly Dictionary<string, string> SpecialEscapeCharacter = new Dictionary<string, string>()
        {
            { EscapeCharacter + ",", ";" }, //line terminator ;
            { EscapeCharacter + "at", "@" }, //command trigger @
            { EscapeCharacter + "_t", "\t" }, //tab
            { EscapeCharacter + "/", "\\"}, // \
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


        public static string OnLoadString(string line, List<string> split)
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

        public static string OnLinkEnd(string line, List<string> split)
        {
            return $"{line}</a>";
        }

        public static string OnImage(string line, List<string> split)
        {
            return $"<img src=\"{line}\" alt=\"{line}\">";
        }

        public static string OnCSS(string line, List<string> split)
        {
            return $"<link rel\"stylesheet\" href=\"line\" type=\"text/css\">";
        }


        public static string OnHeader(string line, List<string> split)
        {
            return $"\n<head>\n{line}\n</head>\n";
        }

        public static string OnHeaderStart(string line, List<string> split)
        {
            if (!string.IsNullOrEmpty(line))
                return $"\n<head>\n{line}";

            return $"\n<head>\n";
        }

        public static string OnHeaderEnd(string line, List<string> split)
        {
            if (!string.IsNullOrEmpty(line))
                return $"{line}\n<head>\n";

            return $"\n</head>\n";
        }


        public static string OnBody(string line, List<string> split)
        {
            return $"\n<body>\n{line}\n</body>\n";
        }

        public static string OnBodyStart(string line, List<string> split)
        {
            if (!string.IsNullOrEmpty(line))
                return $"\n<body>\n{line}";

            return $"\n<body>\n";
        }

        public static string OnBodyEnd(string line, List<string> split)
        {
            if (!string.IsNullOrEmpty(line))
                return $"{line}\n<body>";

            return $"\n</body>\n";
        }


        public static string OnFooter(string line, List<string> split)
        {
            return $"\n<footer>\n{line}\n</footer>\n";
        }

        public static string OnFooterStart(string line, List<string> split)
        {
            if (!string.IsNullOrEmpty(line))
                return $"\n<footer>\n{line}";

            return $"\n<footer>\n";
        }

        public static string OnFooterEnd(string line, List<string> split)
        {
            if (!string.IsNullOrEmpty(line))
                return $"{line}\n<footer>";

            return $"\n</footer>";
        }

        public static string OnCSCode(string line, List<string> split)
        {
            return $"\n@(\n{line}\n)\n";
        }

        public static string OnCSCodeStart(string line, List<string> split)
        {
            return $"\n@(\n{line}";
        }

        public static string OnCSCodeEnd(string line, List<string> split)
        {
            return $"{line}\n)\n";
        }
        #endregion
    }
}
