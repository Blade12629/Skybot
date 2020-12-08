using AutoRefTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace SkyBot.Osu.AutoRef.Data
{
    public class Slot : IEquatable<Slot>, ISlot
    {
        public int Id { get; set; }
        public bool IsReady { get; set; }
        public Uri ProfileUrl { get; set; }
        public string Nickname { get; set; }
        public SlotColor Color { get; set; }
        public string Role { get; set; }
        public bool IsUsed => !string.IsNullOrEmpty(Nickname);

        public List<string> Mods { get; private set; }

        public Slot(int id)
        {
            Id = id;
            Mods = new List<string>();
        }

        public void SetMods(IEnumerable<string> mods)
        {
            Mods.Clear();
            Mods.AddRange(mods);
        }

        public void AddMod(string mod)
        {
            Mods.Add(mod);
        }

        public void Reset()
        {
            IsReady = false;
            ProfileUrl = null;
            Nickname = null;
            Color = SlotColor.None;
            Role = null;

            Mods.Clear();
        }

        public void ResetMods()
        {
            Mods.Clear();
        }

        public void Swap(Slot other)
        {
            bool isReady = other.IsReady;
            Uri profileUrl = other.ProfileUrl;
            string nick = other.Nickname;
            SlotColor color = other.Color;
            string role = other.Role;
            List<string> mods = other.Mods.ToList();

            Move(other);

            IsReady = isReady;
            ProfileUrl = profileUrl;
            Nickname = nick;
            Color = color;
            Role = role;
            Mods = mods.ToList();
        }

        public void Move(Slot other)
        {
            other.IsReady = IsReady;
            other.ProfileUrl = ProfileUrl;
            other.Nickname = Nickname;
            other.Color = Color;
            other.Role = Role;
            other.SetMods(Mods);

            Reset();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Slot);
        }

        public bool Equals([AllowNull] Slot other)
        {
            return other != null &&
                   Nickname == other.Nickname;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Nickname);
        }

        public static bool operator ==(Slot left, Slot right)
        {
            return EqualityComparer<Slot>.Default.Equals(left, right);
        }

        public static bool operator !=(Slot left, Slot right)
        {
            return !(left == right);
        }
    }
}
