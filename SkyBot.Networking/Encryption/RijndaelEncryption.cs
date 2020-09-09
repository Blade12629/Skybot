using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SkyBot.Networking.Encryption
{
    public class RijndaelEncryption : BaseEncryption, IDisposable
    {
        public bool IsDisposed { get; private set; }

        private Rijndael _rijn;

        public RijndaelEncryption(bool generateKeyAndIV) : base(generateKeyAndIV)
        {
            _rijn = Rijndael.Create();
        }

        public RijndaelEncryption(byte[] key, byte[] iv) : base(key, iv)
        {
            _rijn = Rijndael.Create();
        }

        ~RijndaelEncryption()
        {
            Dispose(false);
        }

        public override byte[] Decrypt(byte[] data)
        {
            using (ICryptoTransform transform = _rijn.CreateDecryptor(Key, IV))
            {
                return DoCrypticAction(data, transform);
            }
        }

        public override byte[] Encrypt(byte[] data)
        {
            using (ICryptoTransform transform = _rijn.CreateEncryptor(Key, IV))
            {
                return DoCrypticAction(data, transform);
            }
        }

        public override void GenerateKeyAndIV()
        {
            _rijn.GenerateIV();
            _rijn.GenerateKey();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (disposing)
                _rijn?.Dispose();
        }
    }
}
