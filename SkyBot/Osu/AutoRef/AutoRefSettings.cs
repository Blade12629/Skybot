using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class AutoRefSettings
    {
        public string ScriptFileName { get; set; }
        public bool IsLibrary { get; set; }
        public DateTime CreationDate { get; set; }
        public ulong DiscordGuildId { get; set; }
        public ulong DiscordLogChannelId { get; set; }

        public string ScriptInput { get; set; }
        
        public string LobbyName { get; set; }

        public string SpreadsheetId { get; set; }
        public string SpreadsheetTable { get; set; }

        public AutoRefSettings(string scriptFileName, bool isLibrary, DateTime creationDate, 
                               ulong discordGuildId, ulong discordLogChannelId, string lobbyName, 
                               string scriptInput, string spreadsheetId, string spreadsheetTable)
        {
            ScriptFileName = scriptFileName;
            IsLibrary = isLibrary;
            CreationDate = creationDate;
            DiscordGuildId = discordGuildId;
            DiscordLogChannelId = discordLogChannelId;
            LobbyName = lobbyName;
            ScriptInput = scriptInput;
            SpreadsheetId = spreadsheetId;
            SpreadsheetTable = spreadsheetTable;
        }
    }
}
