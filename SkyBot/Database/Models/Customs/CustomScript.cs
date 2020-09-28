using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models.Customs
{
    public class CustomScript
    {
        public long Id { get; set; }
        public long DiscordGuildId { get; set; }
        public string Name { get; set; }
        public string Script { get; set; }
        public long CreatedBy { get; set; }
        public long LastEditedBy { get; set; }

        public CustomScript(long discordGuildId, string script, string name, long createdBy, long lastEditedBy)
        {
            DiscordGuildId = discordGuildId;
            Name = name;
            Script = script;
            CreatedBy = createdBy;
            LastEditedBy = lastEditedBy;
        }

        public CustomScript()
        {
        }
    }
}
