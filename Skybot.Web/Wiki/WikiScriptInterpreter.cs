using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Skybot.Web.Wiki
{
    public class WikiScriptInterpreter
    {
        public static void TestRun()
        {
            string script = @"Current Time:\n\t@time HH:MM:SS.MSMS;\n@center-st;Future bot @font-color-st red; scripting @font-color-ed;language;\n<br>html supported in scripts<br>some.email\atweb.de";

            script += @"\n\n\ls\ntab \t, < \<, > \>, At symbol: \@ \at";

            WikiScriptInterpreter inp = new WikiScriptInterpreter();
            WikiPage page = inp.Interpret(script);

            page.HtmlToFile(@"D:\reposSSD\SkyBot\Skybot.Web\bin\Debug\netcoreapp3.1\111111html.html");

            Environment.Exit(0);
        }

        private Dictionary<string, WikiScriptCommand> _commands;

        public WikiScriptInterpreter()
        {
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            _commands = new Dictionary<string, WikiScriptCommand>()
            {
                { WikiScriptCommands.Center, new WikiScriptCommand(WikiScriptCommands.Center, WikiScriptCommands.OnCenterStart, WikiScriptCommands.OnCenterEnd, WikiScriptCommands.OnCenterSingle) },
                { WikiScriptCommands.Div, new WikiScriptCommand(WikiScriptCommands.Div, WikiScriptCommands.OnDivStart, WikiScriptCommands.OnDivEnd, WikiScriptCommands.OnDivSingle) },
                { WikiScriptCommands.Paragraph, new WikiScriptCommand(WikiScriptCommands.Paragraph, WikiScriptCommands.OnParagraphStart, WikiScriptCommands.OnParagraphEnd, WikiScriptCommands.OnParagraphSingle) },
                { WikiScriptCommands.LoadFile, new WikiScriptCommand(WikiScriptCommands.LoadFile, null, null, WikiScriptCommands.OnLoadFile) },
                { WikiScriptCommands.CurrentTime, new WikiScriptCommand(WikiScriptCommands.CurrentTime, null, null, WikiScriptCommands.OnCurrentTime) },
                { WikiScriptCommands.CurrentDate, new WikiScriptCommand(WikiScriptCommands.CurrentDate, null, null, WikiScriptCommands.OnCurrentDate) },
                { WikiScriptCommands.CurrentDateTime, new WikiScriptCommand(WikiScriptCommands.CurrentDateTime, null, null, WikiScriptCommands.OnCurrentDateTime) },
                { WikiScriptCommands.Image, new WikiScriptCommand(WikiScriptCommands.Image, null, null, WikiScriptCommands.OnImage) },
                { WikiScriptCommands.Link, new WikiScriptCommand(WikiScriptCommands.Link, WikiScriptCommands.OnLinkStart, WikiScriptCommands.OnLinkEnd, WikiScriptCommands.OnLink) },
            };
        }

        public WikiPage Interpret(string pageScript)
        {
            List<string> split = ReplaceEscapeCharacters(pageScript, WikiScriptCommands.EscapeCharacters).Replace(WikiScriptCommands.CommandCharacter, $"{WikiScriptCommands.CommandTerminator}{WikiScriptCommands.CommandCharacter}")
                                                                                                         .Split(WikiScriptCommands.CommandTerminator[0])
                                                                                                         .Where(l => l != null)
                                                                                                         .ToList();
            StringBuilder htmlBuilder = new StringBuilder();

            for (int i = split.Count - 1; i >= 0; i--)
            {
                if (split[i].Length == 0 ||
                ((split[i][0] == ' ' || split[i].Equals("")) && split[i].Length == 1))
                {
                    split.RemoveAt(i);
                    continue;
                }
            }

            for (int i = 0; i < split.Count; i++)
                htmlBuilder.Append(ReplaceEscapeCharacters(InterpretLine(split[i]), WikiScriptCommands.SpecialEscapeCharacter));

            return new WikiPage(htmlBuilder.ToString(), pageScript);
        }

        private string InterpretLine(string line)
        {
            if (!line.StartsWith(WikiScriptCommands.CommandCharacter))
                return line;

            line = line.TrimStart(WikiScriptCommands.CommandCharacter[0]);

            int index = line.IndexOf(' ');

            bool start = false;
            bool end = false;

            string cmd = index > -1 ? line.Substring(0, index).ToLower(CultureInfo.CurrentCulture) : line;

            if (cmd.EndsWith(WikiScriptCommands.AreaStart))
            {
                start = true;
                cmd = cmd.Remove(cmd.Length - WikiScriptCommands.AreaStart.Length);
            }
            else if (cmd.EndsWith(WikiScriptCommands.AreaEnd))
            {
                end = true;
                cmd = cmd.Remove(cmd.Length - WikiScriptCommands.AreaEnd.Length);
            }

            if (!_commands.ContainsKey(cmd))
                return line;

            WikiScriptCommand wsc = _commands[cmd];

            Func<string, List<string>, string> func;

            if (start)
            {
                func = wsc.OnAreaStart;
                line = line.Remove(0, WikiScriptCommands.AreaStart.Length);
            }
            else if (end)
            {
                func = wsc.OnAreaEnd;
                line = line.Remove(0, WikiScriptCommands.AreaStart.Length);
            }
            else
                func = wsc.OnSingle;

            if (func == null)
                throw new InvalidWikiCommandInvokeException($"Undefined (Available: {WikiScriptCommands.AreaStart}: {wsc.OnAreaStart != null}, {WikiScriptCommands.AreaStart}: {wsc.OnAreaEnd != null}, default: {wsc.OnSingle != null})");

            try
            {
                string removed = line.Remove(0, cmd.Length);

                if (removed.Length > 0 && removed[0] == ' ')
                    removed = removed.Remove(0, 1);

                return func(removed, removed.Split(' ').ToList());
            }
            catch (Exception ex)
            {
                throw new InvalidWikiCommandInvokeException(ex.Message);
            }
        }

        private string ReplaceEscapeCharacters(string input, Dictionary<string, string> escapeCharacters)
        {
            foreach (var ec in escapeCharacters)
                input = input.Replace(ec.Key, ec.Value);

            return input;
        }
    }
}
