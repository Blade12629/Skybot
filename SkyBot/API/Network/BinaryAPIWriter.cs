using Grapevine.Client;
using Grapevine.Interfaces.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkyBot.API.Network
{
    public class BinaryAPIWriter
    {
        public long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        private Stream _stream;

        public BinaryAPIWriter(IHttpContext c) : this((HttpResponse)c.Response)
        {
        }

        public BinaryAPIWriter(HttpResponse response) : this(response.Advanced.OutputStream)
        {
        }

        public BinaryAPIWriter(HttpRequest request) : this(request.Advanced.InputStream)
        {
        }

        public BinaryAPIWriter(RestResponse response) : this(response.Advanced.GetResponseStream())
        {
        }

        public BinaryAPIWriter(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            _stream = stream;
        }

        public byte[] GetDataFromStream()
        {
            byte[] data = new byte[_stream.Length];
            long oldPos = _stream.Position;

            _stream.Position = 0;
            _stream.Read(data, 0, data.Length);
            _stream.Position = oldPos;

            return data;
        }

        public void Write(byte b)
        {
            _stream.WriteByte(b);
        }

        public void Write(ReadOnlySpan<byte> bytes)
        {
            _stream.Write(bytes);
        }

        public void Write(short v)
        {
            Write(BitConverter.GetBytes(v));
        }

        public void Write(ushort v)
        {
            Write(BitConverter.GetBytes(v));
        }

        public void Write(int v)
        {
            Write(BitConverter.GetBytes(v));
        }

        public void Write(uint v)
        {
            Write(BitConverter.GetBytes(v));
        }

        public void Write(long v)
        {
            Write(BitConverter.GetBytes(v));
        }

        public void Write(ulong v)
        {
            Write(BitConverter.GetBytes(v));
        }

        public void Write(string v)
        {
            byte[] data = Encoding.UTF8.GetBytes(v);
            Write(data.Length);

            Write(data);
        }

        public void Write(bool v)
        {
            Write((byte)(v ? 1 : 0));
        }

        public void Write(float v)
        {
            Write(BitConverter.GetBytes(v));
        }

        public void Write(double v)
        {
            Write(BitConverter.GetBytes(v));
        }

        public void Write(DateTime date)
        {
            Write(date.Ticks);
        }

        public void Write(TimeSpan ts)
        {
            Write(ts.Ticks);
        }

        public void Write(DateTimeOffset offset)
        {
            Write(offset.Ticks);
            Write(offset.Offset.Ticks);
        }
    }
}
