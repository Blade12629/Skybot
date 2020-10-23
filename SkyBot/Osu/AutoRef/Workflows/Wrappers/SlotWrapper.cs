using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Workflows.Wrappers
{
    public class SlotWrapper
    {
        public int Id { get; }
        public bool IsReady { get; }
        public Uri ProfileUrl { get; }
        public string Nickname { get; }
        public SlotColor? Color { get; }
        public string Role { get; }
        public bool IsUsed => Nickname != null;

        public List<string> Mods { get; }

        internal SlotWrapper(int id, bool isReady, Uri profileUrl, string nickname, SlotColor? color, string role, List<string> mods)
        {
            Id = id;
            IsReady = isReady;
            ProfileUrl = profileUrl;
            Nickname = nickname;
            Color = color;
            Role = role;
            Mods = mods;
        }

        public static implicit operator SlotWrapper(Slot s)
        {
            return new SlotWrapper(s.Id, s.IsReady, s.ProfileUrl, s.Nickname, s.Color, s.Role, s.Mods);
        }
    }
}
