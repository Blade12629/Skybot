using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SkyBot.Networking.Encryption
{
    public class RSAEncryption : BaseEncryption, IDisposable
    {
        public bool IsDisposed { get; private set; }

        private RSA _rsa;

        public RSAEncryption() : base(false)
        {
            _rsa = RSA.Create();
        }

        public RSAEncryption(string keyAsXml) : this()
        {
            _rsa.FromXmlString(keyAsXml);
        }

        private RSAEncryption(bool generateKeyAndIV) : base(generateKeyAndIV)
        {
        }

        private RSAEncryption(byte[] key, byte[] iv) : base(key, iv)
        {
        }

        ~RSAEncryption()
        {
            Dispose(false);
        }

        public override byte[] Decrypt(byte[] data)
        {
            return _rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
        }

        public override byte[] Encrypt(byte[] data)
        {
            return _rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        }

        public string ToXml(bool withPrivKey)
        {
            return _rsa.ToXmlString(withPrivKey);
        }

        public override void GenerateKeyAndIV()
        {

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
                _rsa?.Dispose();
        }
    }
}
