﻿using AutoRefTypes.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    /// <summary>
    /// Your scripts entry point
    /// </summary>
    public interface IEntryPoint
    {
        /// <summary>
        /// The first thing to be called
        /// </summary>
        public void OnLoad(ILobby lobby, IEventRunner eventRunner);
    }
}
