using Grapevine.Client;
using Grapevine.Interfaces.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkyBot.API.Network
{
    public class BinaryAPIReader
    {
        public long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        private Stream _stream;

        public BinaryAPIReader(HttpRequest request) : this(request.Advanced.InputStream)
        {
        }

        public BinaryAPIReader(HttpResponse response) : this(response.Advanced.OutputStream)
        {
        }

        public BinaryAPIReader(RestResponse response) : this(response.Advanced.GetResponseStream())
        {
        }

        public BinaryAPIReader(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            _stream = stream;
        }

        public byte ReadByte()
        {
            return (byte)_stream.ReadByte();
        }

        public byte[] ReadBytes(int length)
        {
            byte[] data = new byte[length];
            _stream.Read(data, 0, length);

            return data;
        }

        public short ReadShort()
        {
            return BitConverter.ToInt16(ReadBytes(2));
        }

        public ushort ReadUShort()
        {
            return BitConverter.ToUInt16(ReadBytes(2));
        }

        public int ReadInt()
        {
            return BitConverter.ToInt32(ReadBytes(4));
        }

        public uint ReadUInt()
        {
            return BitConverter.ToUInt32(ReadBytes(4));
        }

        public long ReadLong()
        {
            return BitConverter.ToInt64(ReadBytes(8));
        }

        public ulong ReadULong()
        {
            return BitConverter.ToUInt64(ReadBytes(8));
        }

        public string ReadString(int length)
        {
            byte[] data = ReadBytes(length);
            return Encoding.UTF8.GetString(data);
        }

        public string ReadString()
        {
            int length = ReadInt();
            return ReadString(length);
        }

        public bool ReadBool()
        {
            return ReadByte() == 1;
        }

        public float ReadFloat()
        {
            return BitConverter.ToSingle(ReadBytes(4));
        }

        public double ReadDouble()
        {
            return BitConverter.ToDouble(ReadBytes(8));
        }

        public DateTime ReadDate()
        {
            return new DateTime(ReadLong());
        }

        public TimeSpan ReadTimeSpan()
        {
            return new TimeSpan(ReadLong());
        }

        public DateTimeOffset ReadDateOffset()
        {
            return new DateTimeOffset(ReadDate(), ReadTimeSpan());
        }
    }
}
