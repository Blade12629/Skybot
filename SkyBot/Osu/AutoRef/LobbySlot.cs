using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class LobbySlot
    {
        public int Slot { get; }
        public bool IsReady { get; set; }
        public string ProfileUrl { get; set; }
        public string Nickname { get; set; }
        public LobbyColor Team { get; set; }
        public List<string> Mods { get; }
        public string Role { get; set; }

        public LobbySlot(int slot)
        {
            Slot = slot;
            Mods = new List<string>();
        }

        public void Reset()
        {
            IsReady = false;
            ProfileUrl = null;
            Nickname = null;
            Team = LobbyColor.None;
            Mods.Clear();
            Role = null;
        }

        public void Move(LobbySlot other)
        {
            other.IsReady = IsReady;
            other.ProfileUrl = ProfileUrl;
            other.Nickname = Nickname;
            other.Team = Team;
            
            if (other.Mods.Count > 0)
                other.Mods.Clear();

            if (Mods.Count > 0)
                other.Mods.AddRange(Mods);

            Role = other.Role;

            Reset();
        }
    }
}
