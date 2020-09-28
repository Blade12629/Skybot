using DSharpPlus.Entities;
using SkyBot;
using SkyBot.Database.Models.Customs;
using SkyBot.Discord;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace DiscordCommands
{
    public class ScriptCommand : ICommand
    {
        public static Scripting.ScriptingEngine Engine { get; private set; }

        public bool IsDisabled { get; set; }

        public string Command => "script";

        public AccessLevel AccessLevel => AccessLevel.User;

        public bool AllowOverwritingAccessLevel => false;

        public CommandType CommandType => CommandType.None;

        public string Description => "Manage Scripts";

        public string Usage => "{prefix}script list [page, default: 1]\n\nAdmin:\n{prefix}script add \"<scriptname>\" <urlToScriptRaw>\n{prefix}script edit/remove <scriptid>\nDev:\n{prefix}script run <script to run>";

        public int MinParameters => 1;

        public ScriptCommand()
        {
            Engine = new Scripting.ScriptingEngine();
        }

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            string parameterStr = args.ParameterString.Remove(0, args.Parameters[0].Length).TrimStart(' ');

            switch(args.Parameters[0])
            {
                case "run":
                    if (args.Parameters.Count < 2)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.NotEnoughParameters);
                        return;
                    }
                    if (handler.GetAccessLevel(args.Member.Id, args.Guild.Id) < AccessLevel.Dev)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.AccessTooLow);
                        return;
                    }

                    Engine.RunScript(parameterStr, args.Guild, args.Channel, args.Member, client, args.Config);
                    break;

                case "edit":
                    if (args.Parameters.Count < 3)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.NotEnoughParameters);
                        return;
                    }
                    if (handler.GetAccessLevel(args.Member.Id, args.Guild.Id) < AccessLevel.Admin)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.AccessTooLow);
                        return;
                    }
                    {
                        if (!long.TryParse(args.Parameters[1], out long scriptId))
                        {
                            HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, Resources.FailedParseException, "scriptId"));
                            return;
                        }
                        EditScript(client, args.Channel, args.Member, scriptId, parameterStr);
                    }
                    break;

                case "remove":
                case "delete":
                    if (args.Parameters.Count < 2)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.NotEnoughParameters);
                        return;
                    }
                    if (handler.GetAccessLevel(args.Member.Id, args.Guild.Id) < AccessLevel.Admin)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.AccessTooLow);
                        return;
                    }

                    {
                        if (!long.TryParse(args.Parameters[1], out long scriptId))
                        {
                            HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, Resources.FailedParseException, "scriptId"));
                            return;
                        }
                        DeleteScript(client, args.Channel, scriptId);
                    }
                    break;

                case "add":
                    if (args.Parameters.Count < 3)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.NotEnoughParameters);
                        return;
                    }
                    if (handler.GetAccessLevel(args.Member.Id, args.Guild.Id) < AccessLevel.Admin)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.AccessTooLow);
                        return;
                    }

                    {
                        string scriptName = GetScriptName(ref parameterStr);
                        AddScript(client, args.Channel, args.Member, scriptName, parameterStr);
                    }
                    break;

                case "list":
                    int page = 1;

                    if (args.Parameters.Count > 1 && int.TryParse(args.Parameters[1], out int page_))
                        page = page_;

                    SendList(args.Channel, client, page);
                    break;
            }
        }

        string GetScriptName(ref string input)
        {
            int start = input.IndexOf('"', StringComparison.CurrentCultureIgnoreCase);

            if (start == -1)
                return null;

            input = input.Remove(0, start + 1);
            start = input.IndexOf('"', StringComparison.CurrentCultureIgnoreCase);

            if (start == -1)
                return null;

            string result = input.Substring(0, start);
            input = input.Remove(0, start + 1).TrimStart(' ');

            return result;
        }

        void SendList(DiscordChannel channel, DiscordHandler handler, int page)
        {
            using DBContext c = new DBContext();
            EmbedPageBuilder builder = new EmbedPageBuilder(2);
            builder.AddColumn("Id");
            builder.AddColumn("Script Name");

            List<CustomScript> scripts = c.CustomScript.Where(cs => cs.DiscordGuildId == (long)channel.Guild.Id).ToList();

            if (scripts.Count == 0 ||
                scripts.Count < page * 10)
            {
                handler.SendSimpleEmbed(channel, "No scripts found on this page");
                return;
            }

            foreach (CustomScript script in scripts)
            {
                builder.Add("Id", script.Id.ToString(CultureInfo.CurrentCulture));
                builder.Add("Script Name", script.Name);
            }

            DiscordEmbed embed = builder.BuildPage(page);
            channel.SendMessageAsync(embed: embed).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        void DeleteScript(DiscordHandler handler, DiscordChannel channel, long scriptId)
        {
            using DBContext c = new DBContext();
            CustomScript s = c.CustomScript.FirstOrDefault(s => s.DiscordGuildId == (long)channel.Guild.Id &&
                                                                s.Id == scriptId);

            if (s == null)
            {
                handler.SendSimpleEmbed(channel, "Script not found");
                return;
            }

            c.CustomScript.Remove(s);
            c.SaveChanges();

            handler.SendSimpleEmbed(channel, "Deleted script");
        }

        void AddScript(DiscordHandler handler, DiscordChannel channel, DiscordMember member, string scriptName, string urlToScript)
        {
            string script = DownloadScript(urlToScript);

            if (script == null)
            {
                HelpCommand.ShowHelp(channel, this, "Failed to download script");
                return;
            }

            using DBContext c = new DBContext();
            c.CustomScript.Add(new CustomScript((long)channel.Guild.Id, scriptName, script, (long)member.Id, (long)member.Id));

            c.SaveChanges();

            handler.SendSimpleEmbed(channel, "Added script");
        }

        void EditScript(DiscordHandler handler, DiscordChannel channel, DiscordMember member, long scriptId, string urlToNewScript)
        {
            string script = DownloadScript(urlToNewScript);

            if (script == null)
            {
                HelpCommand.ShowHelp(channel, this, "Failed to download script");
                return;
            }

            using DBContext c = new DBContext();
            CustomScript s = c.CustomScript.FirstOrDefault(s => s.DiscordGuildId == (long)channel.Guild.Id &&
                                                                s.Id == scriptId);

            if (s == null)
            {
                handler.SendSimpleEmbed(channel, "Script not found");
                return;
            }

            s.LastEditedBy = (long)member.Id;
            s.Script = script;
            c.CustomScript.Update(s);

            c.SaveChanges();

            handler.SendSimpleEmbed(channel, "Updated script");
        }

        string DownloadScript(string url)
        {
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out Uri uri))
                return null;

            using WebClient wc = new WebClient();

            return wc.DownloadString(uri);
        }
    }
}
