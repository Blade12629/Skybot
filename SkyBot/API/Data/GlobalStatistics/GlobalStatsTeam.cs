using SkyBot.API.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.API.Data.GlobalStatistics
{
    public class GlobalStatsTeam : IBinaryAPISerializable
    {
        public int Placement { get; set; }
        public string Name { get; set; }
        public List<long> OsuUserIds { get; }

        public GlobalStatsTeam(int placement, string name) : this(placement, name, new List<long>())
        {
        }

        public GlobalStatsTeam(int placement, string name, List<long> osuUserIds)
        {
            Placement = placement;
            Name = name;
            OsuUserIds = osuUserIds;
        }

        public GlobalStatsTeam()
        {
            OsuUserIds = new List<long>();
        }

        public void Serialize(BinaryAPIWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write(Placement);
            writer.Write(Name);

            writer.Write(OsuUserIds.Count);
            for (int i = 0; i < OsuUserIds.Count; i++)
                writer.Write(OsuUserIds[i]);
        }

        public void Deserialize(BinaryAPIReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Placement = reader.ReadInt();
            Name = reader.ReadString();

            int users = reader.ReadInt();
            for (int i = 0; i < users; i++)
                OsuUserIds.Add(reader.ReadLong());
        }
    }
}
