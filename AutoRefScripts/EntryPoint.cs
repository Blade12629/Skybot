using AutoRefTypes;
using AutoRefTypes.Events;
using AutoRefTypes.Extended.Requests;
using AutoRefTypes.Google.SpreadSheets;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefScripts
{
    public class EntryPoint : IEntryPoint
    {
        ScriptInput _scriptInput;
        Main _main;

        public void OnLoad(ILobby lobby, IEventRunner eventRunner, IDiscordHandler discord, ISpreadsheet sheet, string scriptInputJson)
        {
            _scriptInput = Newtonsoft.Json.JsonConvert.DeserializeObject<ScriptInput>(scriptInputJson);
            _main = new Main(lobby, eventRunner, discord, sheet, _scriptInput);
        }
    }
}
