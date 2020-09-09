using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyBot.API.Network
{
    public static class BinaryAPIHeader
    {
        private static readonly byte[] _HEAD_START = new byte[] { 0xFF, 0x00, 0xFF, 0x00 };
        private static readonly byte[] _HEAD_END = new byte[] { 0x00, 0xFF, 0x00, 0xFF };

        public static void WriteHeader(BinaryAPIWriter writer, byte[] data)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            else if (data == null)
                throw new ArgumentNullException(nameof(data));

            writer.Write(_HEAD_START);

            writer.Write(data.Length);
            writer.Write(data);

            writer.Write(_HEAD_END);
        }

        public static byte[] ReadHeader(BinaryAPIReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (!CheckHead(reader, false))
                return Array.Empty<byte>();

            int headerDataLength = reader.ReadInt();
            byte[] headerData = reader.ReadBytes(headerDataLength);

            if (!CheckHead(reader, true))
                return Array.Empty<byte>();

            return headerData;
        }

        private static bool CheckHead(BinaryAPIReader reader, bool endOfHead)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            byte[] data = reader.ReadBytes(3);

            return Enumerable.SequenceEqual(data, endOfHead ? _HEAD_END : _HEAD_START);
        }
    }
}
