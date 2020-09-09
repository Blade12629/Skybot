using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SkyBot.Networking.Encryption
{
    public abstract class BaseEncryption : IEncryption
    {
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }

        /// <summary>
        /// Initializes this class and calls <see cref="GenerateKeyAndIV"/>
        /// </summary>
        public BaseEncryption(bool generateKeyAndIV)
        {
            if (generateKeyAndIV)
                GenerateKeyAndIV();
        }

        public BaseEncryption(byte[] key, byte[] iv)
        {
            Key = key;
            IV = iv;
        }

        public abstract byte[] Decrypt(byte[] data);
        public abstract byte[] Encrypt(byte[] data);
        public abstract void GenerateKeyAndIV();

        protected byte[] DoCrypticAction(byte[] data, ICryptoTransform transform)
        {
            using MemoryStream outStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(outStream, transform, CryptoStreamMode.Write);

            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            return outStream.ToArray();
        }
    }
}
