using DSharpPlus.Entities;
using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Discord.CommandSystem;
using SkyBot.TicketSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DiscordCommands
{
    public class TicketCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => ResourcesCommands.TicketCommand;

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.Public;

        public string Description => ResourcesCommands.TicketCommandDescription;

        public string Usage => ResourcesCommands.TicketCommandUsage;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            switch (args.Parameters[0].ToLower(CultureInfo.CurrentCulture))
            {
                default:
                    break;

                case "-get":
                    args.Parameters.RemoveAt(0);
                    OnGet(args.Parameters, args);
                    return;

                case "-set":
                    args.Parameters.RemoveAt(0);
                    OnSet(args.Parameters, args);
                    return;

                case "-priority":
                    args.Channel.SendMessageAsync("Priority:\n```\nLow = 0\nNormal = 1\nHigh = 2\nASAP = 3\n```");
                    return;

                case "-status":
                    args.Channel.SendMessageAsync("Priority:\n```\nUnread = 0\nOpen = 1\nNeedsConfirmation = 2\nWIP = 3\nClosed = 4\nArchived = 5\n```");
                    return;

                case "-tag":
                    args.Channel.SendMessageAsync("Priority:\n```\nNone = 0\nCommentator = 1\nStreamer = 2\nMappoolSelector = 4\nReferee = 8\nDeveloper = 16\nOrganizer = 32\n```");
                    return;
            }

            OnAdd(args);
        }

        private void OnAdd(CommandEventArg e)
        {
            if (e.Parameters.Count == 0)
            {
                HelpCommand.ShowHelp(e.Channel, this);
                return;
            }

            StringBuilder msgBuilder = new StringBuilder(e.Parameters[0]);

            for (int i = 1; i < e.Parameters.Count; i++)
                msgBuilder.Append(" " + e.Parameters[i]);

            Ticket ticket = new Ticket((long)e.User.Id, (long)e.Guild.Id, 0, 0, 1, DateTime.Now, msgBuilder.ToString());

            using (DBContext c = new DBContext())
            {
                ticket = c.Ticket.Add(ticket).Entity;
                c.SaveChanges();
            }

            var duser = e.User;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = $"New ticket by {duser.Username}#{duser.Discriminator} ({duser.Mention} (copy&paste for mention))",
                Description = $"ID: {ticket.Id}"
            };

            builder.AddField("Message", ticket.Message);

            DiscordChannel ticketChannel = e.Guild.GetChannel(GetTicketRoomId(e.Guild));

            ticketChannel.SendMessageAsync(embed: builder.Build());

            e.Message.DeleteAsync("Ticket submitted").Wait();

            System.Threading.Tasks.Task.Run(() =>
            {
                e.Member.CreateDmChannelAsync().ConfigureAwait(false).GetAwaiter().GetResult()
                        .SendMessageAsync($"Submitted your ticket");
            });
        }

        private static ulong GetTicketRoomId(DiscordGuild guild)
        {
            using DBContext c = new DBContext();
            DiscordGuildConfig dgc = c.DiscordGuildConfig.FirstOrDefault(dgc => dgc.GuildId == (long)guild.Id);

            if (dgc == null)
                return 0;

            return (ulong)dgc.TicketDiscordChannelId;
        }

        private void OnSet(List<string> split, CommandEventArg e)
        {
            AccessLevel access = e.AccessLevel;

            if (access < AccessLevel.Moderator ||
                !long.TryParse(split[0], out long ticketId))
            {
                HelpCommand.ShowHelp(e.Channel, this);
                return;
            }

            split.RemoveAt(0);
            Ticket ticket;
            using (DBContext c = new DBContext())
                ticket = c.Ticket.FirstOrDefault(b => b.Id == ticketId);

            if (ticket == null)
            {
                e.Channel.SendMessageAsync($"Could not find ticket {ticketId}");
                return;
            }

            bool changed = false;

            for (int i = 0; i < split.Count; i++)
            {
                switch (split[0].ToLower(CultureInfo.CurrentCulture))
                {
                    case "-status":
                    case "-ticketstatus":
                        if (Enum.TryParse(split[i + 1], out TicketStatus status_))
                        {
                            ticket.Status = (short)status_;
                            i++;
                            changed = true;
                            break;
                        }
                        else if (short.TryParse(split[i + 1], out short status_s))
                        {
                            ticket.Status = status_s;
                            i++;
                            changed = true;
                            break;
                        }
                        break;

                    case "-priority":
                    case "-ticketpriority":
                        if (Enum.TryParse(split[i + 1], out TicketPriority prio_))
                        {
                            ticket.Priority = (short)prio_;
                            i++;
                            changed = true;
                            break;
                        }
                        else if (short.TryParse(split[i + 1], out short prio_s))
                        {
                            ticket.Priority = prio_s;
                            i++;
                            changed = true;
                            break;
                        }
                        break;

                    case "-tag":
                    case "-tickettag":
                        if (Enum.TryParse(split[i + 1], out TicketTag tag_))
                        {
                            ticket.Tag = (short)tag_;
                            i++;
                            changed = true;
                            break;
                        }
                        else if (short.TryParse(split[i + 1], out short tag_s))
                        {
                            ticket.Tag = tag_s;
                            i++;
                            changed = true;
                            break;
                        }
                        break;
                }
            }

            if (!changed)
            {
                e.Channel.SendMessageAsync("You did not change anything");
                return;
            }

            using (DBContext c = new DBContext())
            {
                c.Ticket.Update(ticket);
                c.SaveChanges();
            }

            e.Channel.SendMessageAsync("Updated ticket");
        }

        private void OnGet(List<string> split, CommandEventArg e)
        {
            AccessLevel access = e.AccessLevel;

            List<Ticket> tickets = new List<Ticket>();
            StringBuilder searchOptionsbuilder = null;

            //User search
            if (access < AccessLevel.Moderator)
            {
                using (DBContext c = new DBContext())
                    tickets.AddRange(c.Ticket.Where(b => b.DiscordId == (long)e.User.Id &&
                                                         b.DiscordGuildId == (long)e.Guild.Id));
            }
            else //Staff search
            {
                long? discordId = null;
                long? id = null;
                TicketTag? tag = null;
                TicketStatus? status = null;
                TicketPriority? priority = null;
                bool sortByNewest = false;
                bool sortByOldest = false;
                searchOptionsbuilder = new StringBuilder();


                for (int i = 2; i < split.Count; i++)
                {
                    if (!split[i].StartsWith('-'))
                        break;

                    switch (split[i].ToLower(CultureInfo.CurrentCulture))
                    {
                        case "-ticketid":
                        case "-id":
                            if (!long.TryParse(split[i + 1], out long id_))
                            {
                                HelpCommand.ShowHelp(e.Channel, this);
                                return;
                            }

                            id = id_;
                            searchOptionsbuilder.Append("id ");
                            searchOptionsbuilder.Append(id);
                            searchOptionsbuilder.Append(" | ");
                            i++;
                            break;

                        case "-did":
                        case "-uid":
                        case "-userid":
                        case "-discordid":
                            if (!long.TryParse(split[i + 1], out long dId_))
                            {
                                HelpCommand.ShowHelp(e.Channel, this);
                                return;
                            }

                            discordId = dId_;
                            searchOptionsbuilder.Append("dId ");
                            searchOptionsbuilder.Append(dId_);
                            searchOptionsbuilder.Append(" | ");
                            i++;
                            break;

                        case "-tag":
                        case "-tickettag":
                            if (Enum.TryParse(split[i + 1], out TicketTag tag_))
                            {
                                tag = tag_;
                                searchOptionsbuilder.Append("tag ");
                                searchOptionsbuilder.Append(tag.Value.ToString());
                                searchOptionsbuilder.Append(" | ");
                                i++;
                                break;
                            }
                            else if (short.TryParse(split[i + 1], out short tag_s))
                            {
                                tag = (TicketTag)tag_s;
                                searchOptionsbuilder.Append("tag ");
                                searchOptionsbuilder.Append(tag.Value.ToString());
                                searchOptionsbuilder.Append(" | ");
                                i++;
                                break;
                            }

                            HelpCommand.ShowHelp(e.Channel, this);
                            return;

                        case "-status":
                        case "-ticketstatus":
                            if (Enum.TryParse(split[i + 1], out TicketStatus status_))
                            {
                                status = status_;
                                searchOptionsbuilder.Append("status ");
                                searchOptionsbuilder.Append(status.Value.ToString());
                                searchOptionsbuilder.Append(" | ");
                                i++;
                                break;
                            }
                            else if (short.TryParse(split[i + 1], out short status_s))
                            {
                                status = (TicketStatus)status_s;
                                searchOptionsbuilder.Append("status ");
                                searchOptionsbuilder.Append(status.Value.ToString());
                                searchOptionsbuilder.Append(" | ");
                                i++;
                                break;
                            }

                            HelpCommand.ShowHelp(e.Channel, this);
                            return;

                        case "-priority":
                        case "-ticketpriority":
                            if (Enum.TryParse(split[i + 1], out TicketPriority prio_))
                            {
                                priority = prio_;
                                searchOptionsbuilder.Append("priority ");
                                searchOptionsbuilder.Append(priority.Value.ToString());
                                searchOptionsbuilder.Append(" | ");
                                i++;
                                break;
                            }
                            else if (short.TryParse(split[i + 1], out short prio_s))
                            {
                                priority = (TicketPriority)prio_s;
                                searchOptionsbuilder.Append("priority ");
                                searchOptionsbuilder.Append(priority.Value.ToString());
                                searchOptionsbuilder.Append(" | ");
                                i++;
                                break;
                            }

                            HelpCommand.ShowHelp(e.Channel, this);
                            return;

                        case "-sortByNewest":
                        case "-sbn":
                            sortByNewest = true;
                            searchOptionsbuilder.Append("sortByNewest |");
                            break;

                        case "-sortByOldest":
                        case "-sbo":
                            sortByOldest = true;
                            searchOptionsbuilder.Append("sortByOldest |");
                            break;
                    }
                }

                if (split[0].ToLower(CultureInfo.CurrentCulture) == "s")
                {
                    if (!discordId.HasValue && !id.HasValue)
                    {
                        e.Channel.SendMessageAsync("!ticket s page -id(+value, Ticket Id) -uid(+value, Discord User Id)");
                        return;
                    }

                    tickets.Add(TicketHandler.GetTicket(id, discordId));
                }
                else if (split[0].ToLower(CultureInfo.CurrentCulture).Equals("m", StringComparison.CurrentCultureIgnoreCase))
                    tickets.AddRange(TicketHandler.GetTickets(discordId, (long?)e.Guild.Id, id, status,
                                                              priority, tag, sortByNewest,
                                                              sortByOldest));
            }

            if (tickets.Count == 0)
            {
                e.Channel.SendMessageAsync("Could not find any tickets");
                return;
            }

            int page = 0;

            if (int.TryParse(split[1], out int page_))
                page = page_;
            
            int pageStart = GetPageIndexStart(page);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = "Ticketlist",
            };

            if (searchOptionsbuilder != null)
                builder.Description = "Search options: " + searchOptionsbuilder.ToString();

            if (tickets.Count > 1)
            {
                StringBuilder idBuilder = new StringBuilder();
                StringBuilder statusBuilder = new StringBuilder();
                StringBuilder priorityBuilder = new StringBuilder();

                for (int i = pageStart; i < pageStart + 10; i++)
                {
                    if (i >= tickets.Count)
                        break;

                    idBuilder.AppendLine(tickets[i].Id.ToString(CultureInfo.CurrentCulture));
                    statusBuilder.AppendLine(((TicketStatus)tickets[i].Status).ToString());
                    priorityBuilder.AppendLine(((TicketPriority)tickets[i].Priority).ToString());
                }

                builder.AddField("ID", idBuilder.ToString(), true);
                builder.AddField("Status", statusBuilder.ToString(), true);
                builder.AddField("Priority", priorityBuilder.ToString(), true);
            }
            else
            {
                var duser = e.User;

                builder.Timestamp = tickets[0].Timestamp;

                builder.AddField("Ticket Id", tickets[0].Id.ToString(CultureInfo.CurrentCulture), true);
                builder.AddField("Discord User", $"{duser.Username}#{duser.Discriminator} ({duser.Mention})", true);
                builder.AddField("Message", tickets[0].Message);

                builder.AddField("Settings", $"Priority: {(TicketPriority)tickets[0].Priority}\nTag: {(TicketTag)tickets[0].Tag}\nStatus: {(TicketStatus)tickets[0].Status}");
            }

            var embed = builder.Build();

            e.Channel.SendMessageAsync(embed: embed);
        }

        private static int GetPageIndexStart(int page, int countPerPage = 10)
        {
            if (page == 0)
                return 0;

            return page * countPerPage - 1;
        }

        private static int GetTotalPages(int count, int countPerPage = 10)
        {
            if (count <= countPerPage)
                return 1;

            double c = (double)count / countPerPage;
            int c2 = (int)c;

            if (c > c2)
                c2++;

            return c2;
        }
    }
}
