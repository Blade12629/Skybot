using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.API.Network
{
    public interface IBinaryAPISerializable
    {
        void Serialize(BinaryAPIWriter writer);
        void Deserialize(BinaryAPIReader reader);
    }
}
