﻿using SkyBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyBot.TicketSystem
{
    public static class TicketHandler
    {
        public static Ticket GetTicket(long? id = null, long? discordId = null)
        {
            List<Func<Ticket, bool>> filters = new List<Func<Ticket, bool>>();

            if (id.HasValue)
                filters.Add(new Func<Ticket, bool>(b => b.Id == id.Value));
            if (discordId.HasValue)
                filters.Add(new Func<Ticket, bool>(b => b.DiscordId == discordId.Value));

            if (filters.Count == 0)
                return null;

            using (DBContext c = new DBContext())
                return c.Ticket.FirstOrDefault(BuildSearchFilter(filters));
        }

        public static List<Ticket> GetTickets(long? discordId = null, long? discordGuildId = null, long? id = null, TicketStatus? status = null,
                                          TicketPriority? priority = null, TicketTag? tag = null,
                                          bool sortByNewest = false, bool sortByOldest = false)
        {
            List<Func<Ticket, bool>> filters = new List<Func<Ticket, bool>>();

            if (discordGuildId.HasValue)
                filters.Add(new Func<Ticket, bool>(b => b.DiscordGuildId == discordGuildId.Value));
            if (discordId.HasValue)
                filters.Add(new Func<Ticket, bool>(b => b.DiscordId == discordId.Value));
            if (id.HasValue)
                filters.Add(new Func<Ticket, bool>(b => b.Id == id.Value));
            if (status.HasValue)
                filters.Add(new Func<Ticket, bool>(b => b.Status == (short)status.Value));
            if (priority.HasValue)
                filters.Add(new Func<Ticket, bool>(b => b.Priority == (short)priority.Value));
            if (tag.HasValue)
                filters.Add(new Func<Ticket, bool>(b => b.Tag == (short)tag.Value));

            if (filters.Count == 0)
                return null;

            List<Ticket> tickets = new List<Ticket>();
            using (DBContext c = new DBContext())
                tickets.AddRange(c.Ticket.Where(BuildSearchFilter(filters)));

            if (tickets.Count == 0)
                return null;

            if (sortByNewest)
                tickets = tickets.OrderByDescending(b => b.Timestamp).ToList();
            else if (sortByOldest)
                tickets = tickets.OrderBy(b => b.Timestamp).ToList();


            return tickets;
        }

        private static Func<T, bool> BuildSearchFilter<T>(List<Func<T, bool>> input)
        {
            Func<T, bool> result = input[0];

            for (int i = 1; i < input.Count; i++)
                result += input[i];

            return result;
        }

        public static void SubmitTicket(long discordId, long discordGuildId, string message, DateTime timestamp, TicketStatus? status = null,
                                 TicketPriority? priority = null, TicketTag? tag = null)
        {
            Ticket ticket = new Ticket(discordId, discordGuildId, (short)(tag ?? 0), (short)(status ?? 0), (short)(priority ?? 0), timestamp, message);

            using (DBContext c = new DBContext())
            {
                c.Ticket.Add(ticket);
                c.SaveChanges();
            }
        }
    }
}
