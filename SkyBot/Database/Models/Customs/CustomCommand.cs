using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models.Customs
{
    public class CustomCommand
    {
        public long Id { get; set; }
        public long DiscordGuildId { get; set; }
        public long ScriptId { get; set; }
        public bool IsEnabled { get; set; }

        public int AccessLevel { get; set; }
        public string Command { get; set; }
        public int MinParameter { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }

        public CustomCommand(long discordGuildId, long scriptId, bool isEnabled, int accessLevel, 
                             string command, int minParameter, string description, string usage)
        {
            DiscordGuildId = discordGuildId;
            ScriptId = scriptId;
            IsEnabled = isEnabled;
            AccessLevel = accessLevel;
            Command = command;
            MinParameter = minParameter;
            Description = description;
            Usage = usage;
        }

        public CustomCommand()
        {
        }
    }
}
