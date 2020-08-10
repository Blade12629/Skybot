using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class ByteTable
    {
        public long Id { get; set; }
        public string Identifier { get; set; }
        public byte[] Data { get; set; }

        public ByteTable(string identifier, byte[] data)
        {
            Identifier = identifier;
            Data = data;
        }

        public ByteTable()
        {
        }
    }
}
