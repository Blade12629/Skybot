using AutoRefTypes.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    public interface IEntryPoint
    {
        public void OnLoad(ILobby lobby, IEventRunner eventRunner);
    }
}
