using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class ByteTable : IEquatable<ByteTable>
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

        public override bool Equals(object obj)
        {
            return Equals(obj as ByteTable);
        }

        public bool Equals([AllowNull] ByteTable other)
        {
            return other != null &&
                   Id == other.Id &&
                   Identifier == other.Identifier;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Identifier);
        }

        public static bool operator ==(ByteTable left, ByteTable right)
        {
            return EqualityComparer<ByteTable>.Default.Equals(left, right);
        }

        public static bool operator !=(ByteTable left, ByteTable right)
        {
            return !(left == right);
        }
    }
}
