using DSharpPlus.Entities;
using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace DiscordCommands
{
    public class RemindMeCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "remindme";

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;

        private static Timer _remindMeTimer;

        public RemindMeCommand()
        {
            _remindMeTimer = new Timer(10000)
            {
                AutoReset = true,
            };
            _remindMeTimer.Elapsed += OnRemindMeTimerElapsed;
        }

        private void OnRemindMeTimerElapsed(object sender, ElapsedEventArgs e)
        {
            using DBContext c = new DBContext();
            var reminders = c.Reminder.Where(r => r.EndDate <= DateTime.UtcNow).ToList();

            try
            {
                for (int i = 0; i < reminders.Count; i++)
                {
                    c.Reminder.Remove(reminders[i]);

                    var dchannel = Program.DiscordHandler.Client.GetChannelAsync((ulong)reminders[i].DiscordChannelId).Result;
                    var duser = Program.DiscordHandler.Client.GetUserAsync((ulong)reminders[i].DiscordUserId).Result;

                    dchannel.SendMessageAsync($"Reminder for you {duser.Mention}:\n{reminders[i].Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, LogLevel.Error);
            }

            c.SaveChanges();
        }

        ~RemindMeCommand()
        {
            try
            {
                _remindMeTimer?.Stop();
                _remindMeTimer?.Dispose();
            }
            catch (Exception)
            {

            }
        }

        public string Description => "Sets a timer to remind you";

        public string Usage => "!remindme list [page, default: 1]\n!remindme remove <reminderId>\n!remindme <days>:<minutes>:<hours> <message>\n(Max: 62 days)";

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count == 0)
            {
                HelpCommand.ShowHelp(args.Channel, this);
                return;
            }

            switch (args.Parameters[0].ToLower(System.Globalization.CultureInfo.CurrentCulture))
            {
                case "list":
                    args.Parameters.RemoveAt(0);
                    ShowList(args);
                    return;
                case "remove":
                    args.Parameters.RemoveAt(0);
                    Remove(args);
                    return;

                default:
                    if (args.Parameters.Count < 2)
                    {
                        HelpCommand.ShowHelp(args.Channel, this);
                        return;
                    }
                    break;
            }

            DateTime? date = ParseDate(args.Parameters, 0, 62);

            if (!date.HasValue)
            {
                HelpCommand.ShowHelp(args.Channel, this, "Could not parse date");
                return;
            }

            args.Parameters.RemoveAt(0);

            StringBuilder msg = new StringBuilder(args.Parameters[0]);

            for (int i = 1; i < args.Parameters.Count; i++)
                msg.Append(' ' + args.Parameters[i]);

            using DBContext c = new DBContext();
            var reminder = c.Reminder.Add(new SkyBot.Database.Models.Reminder((long)args.User.Id, (long)args.Channel.Id, msg.ToString(), date.Value)).Entity;
            c.SaveChanges();

            args.Channel.SendMessageAsync("Created your reminder with id " + reminder.Id);
        }

        private void ShowList(CommandEventArg args)
        {
            int page = 1;
            if (args.Parameters.Count > 0 && int.TryParse(args.Parameters[0], out int page_))
                page = page_;

            using DBContext c = new DBContext();
            var reminders = c.Reminder.Where(r => r.DiscordUserId == (long)args.User.Id &&
                                                  r.EndDate > DateTime.UtcNow).ToList();

            double maxPages_ = reminders.Count / 10;
            int maxPages = (int)maxPages_;

            if (maxPages_ > maxPages)
                maxPages++;


            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = "Reminder list",
                Description = $"Page {page}/{maxPages}"
            };
            page--;

            StringBuilder mb = new StringBuilder();
            StringBuilder idb = new StringBuilder();
            StringBuilder edb = new StringBuilder();

            for (int i = 0; i < reminders.Count; i++)
            {
                if (reminders[i].Message.Length > 50)
                    mb.AppendLine(reminders[i].Message.Substring(0, 50) + "...");
                else
                    mb.AppendLine(reminders[i].Message);

                idb.AppendLine(reminders[i].Id.ToString(System.Globalization.CultureInfo.CurrentCulture));
                edb.AppendLine(reminders[i].EndDate.ToString(System.Globalization.CultureInfo.CurrentCulture));
            }

            builder.AddField("ID", idb.ToString(), true);
            builder.AddField("EndDate", idb.ToString(), true);
            builder.AddField("Message", idb.ToString(), true);

            args.Channel.SendMessageAsync(embed: builder.Build());
        }

        private void Remove(CommandEventArg args)
        {
            if (!long.TryParse(args.Parameters[0], out long timerId))
            {
                HelpCommand.ShowHelp(args.Channel, this, "Failed to parse Reminder id");
                return;
            }

            using DBContext c = new DBContext();
            var reminder = c.Reminder.FirstOrDefault(r => r.DiscordUserId == (long)args.User.Id &&
                                                          r.Id == timerId);

            if (reminder == null)
            {
                args.Channel.SendMessageAsync($"Reminder with id {timerId} not found");
                return;
            }

            c.Reminder.Remove(reminder);
            c.SaveChanges();

            args.Channel.SendMessageAsync("Removed reminder with id " + timerId);
        }

        private DateTime? ParseDate(List<string> parameters, int index, int maxDays)
        {
            string[] dateSplit = parameters[index].Split(':');

            if (!int.TryParse(dateSplit[0], out int days) || 
                !int.TryParse(dateSplit[1], out int hours) ||
                !int.TryParse(dateSplit[1], out int minutes))
                return null;

            TimeSpan validationTS = new TimeSpan(days, hours, minutes, 0);

            if (validationTS.TotalDays > maxDays)
                return DateTime.UtcNow.AddDays(60);
            else
                return DateTime.UtcNow.AddDays(validationTS.TotalDays);
        }
    }
}
