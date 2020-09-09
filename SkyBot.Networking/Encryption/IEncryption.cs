using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Networking.Encryption
{
    public interface IEncryption
    {
        byte[] Key { get; set; }
        byte[] IV { get; set; }

        byte[] Encrypt(byte[] data);
        byte[] Decrypt(byte[] data);
    }
}
