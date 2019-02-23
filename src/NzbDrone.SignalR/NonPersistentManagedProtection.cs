using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;


namespace NzbDrone.SignalR
{
    // Copied from https://github.com/mono/mono/blob/c5b88ec4f323f2bdb7c7d0a595ece28dae66579c/mcs/class/System.Security/Mono.Security.Cryptography/ManagedProtection.cs
    // And modified to use an non-persistent encryption key

    //
    // ManagedProtection.cs - 
    //	Protect (encrypt) data without (user involved) key management
    //
    // Author:
    //	Sebastien Pouliot  <sebastien@ximian.com>
    //
    // Copyright (C) 2005 Novell, Inc (http://www.novell.com)
    //
    // Permission is hereby granted, free of charge, to any person obtaining
    // a copy of this software and associated documentation files (the
    // "Software"), to deal in the Software without restriction, including
    // without limitation the rights to use, copy, modify, merge, publish,
    // distribute, sublicense, and/or sell copies of the Software, and to
    // permit persons to whom the Software is furnished to do so, subject to
    // the following conditions:
    // 
    // The above copyright notice and this permission notice shall be
    // included in all copies or substantial portions of the Software.
    // 
    // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    // EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    // MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    // NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
    // LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
    // OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
    // WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    //
    internal static class NonPersistentManagedProtection
    {
        public static byte[] Protect(byte[] userData, byte[] optionalEntropy)
        {
            if (userData == null)
                throw new ArgumentNullException("userData");

            Rijndael aes = Rijndael.Create();
            aes.KeySize = 128;

            byte[] encdata = null;
            using (MemoryStream ms = new MemoryStream())
            {
                ICryptoTransform t = aes.CreateEncryptor();
                using (CryptoStream cs = new CryptoStream(ms, t, CryptoStreamMode.Write))
                {
                    cs.Write(userData, 0, userData.Length);
                    cs.Close();
                    encdata = ms.ToArray();
                }
            }

            byte[] key = null;
            byte[] iv = null;
            byte[] secret = null;
            byte[] header = null;
            SHA256 hash = SHA256.Create();

            try
            {
                key = aes.Key;
                iv = aes.IV;
                secret = new byte[1 + 1 + 16 + 1 + 16 + 1 + 32];

                byte[] digest = hash.ComputeHash(userData);
                if ((optionalEntropy != null) && (optionalEntropy.Length > 0))
                {
                    // the same optionalEntropy will be required to get the data back
                    byte[] mask = hash.ComputeHash(optionalEntropy);
                    for (int i = 0; i < 16; i++)
                    {
                        key[i] ^= mask[i];
                        iv[i] ^= mask[i + 16];
                    }
                    secret[0] = 2; // entropy
                }
                else
                {
                    secret[0] = 1; // without entropy
                }

                secret[1] = 16; // key size
                Buffer.BlockCopy(key, 0, secret, 2, 16);
                secret[18] = 16; // iv size
                Buffer.BlockCopy(iv, 0, secret, 19, 16);
                secret[35] = 32; // digest size
                Buffer.BlockCopy(digest, 0, secret, 36, 32);

                RSAOAEPKeyExchangeFormatter formatter = new RSAOAEPKeyExchangeFormatter(GetKey());
                header = formatter.CreateKeyExchange(secret);
            }
            finally
            {
                if (key != null)
                {
                    Array.Clear(key, 0, key.Length);
                    key = null;
                }
                if (secret != null)
                {
                    Array.Clear(secret, 0, secret.Length);
                    secret = null;
                }
                if (iv != null)
                {
                    Array.Clear(iv, 0, iv.Length);
                    iv = null;
                }
                aes.Clear();
                hash.Clear();
            }

            byte[] result = new byte[header.Length + encdata.Length];
            Buffer.BlockCopy(header, 0, result, 0, header.Length);
            Buffer.BlockCopy(encdata, 0, result, header.Length, encdata.Length);
            return result;
        }

        public static byte[] Unprotect(byte[] encryptedData, byte[] optionalEntropy)
        {
            if (encryptedData == null)
                throw new ArgumentNullException("encryptedData");

            byte[] decdata = null;

            Rijndael aes = Rijndael.Create();
            RSA rsa = GetKey();
            int headerSize = (rsa.KeySize >> 3);
            bool valid1 = (encryptedData.Length >= headerSize);
            if (!valid1)
                headerSize = encryptedData.Length;

            byte[] header = new byte[headerSize];
            Buffer.BlockCopy(encryptedData, 0, header, 0, headerSize);

            byte[] secret = null;
            byte[] key = null;
            byte[] iv = null;
            bool valid2 = false;
            bool valid3 = false;
            bool valid4 = false;
            SHA256 hash = SHA256.Create();

            try
            {
                try
                {
                    RSAOAEPKeyExchangeDeformatter deformatter = new RSAOAEPKeyExchangeDeformatter(rsa);
                    secret = deformatter.DecryptKeyExchange(header);
                    valid2 = (secret.Length == 68);
                }
                catch
                {
                    valid2 = false;
                }

                if (!valid2)
                    secret = new byte[68];

                // known values for structure (version 1 or 2)
                valid3 = ((secret[1] == 16) && (secret[18] == 16) && (secret[35] == 32));

                key = new byte[16];
                Buffer.BlockCopy(secret, 2, key, 0, 16);
                iv = new byte[16];
                Buffer.BlockCopy(secret, 19, iv, 0, 16);

                if ((optionalEntropy != null) && (optionalEntropy.Length > 0))
                {
                    // the decrypted data won't be valid if the entropy isn't
                    // the same as the one used to protect (encrypt) it
                    byte[] mask = hash.ComputeHash(optionalEntropy);
                    for (int i = 0; i < 16; i++)
                    {
                        key[i] ^= mask[i];
                        iv[i] ^= mask[i + 16];
                    }
                    valid3 &= (secret[0] == 2); // with entropy
                }
                else
                {
                    valid3 &= (secret[0] == 1); // without entropy
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    ICryptoTransform t = aes.CreateDecryptor(key, iv);
                    using (CryptoStream cs = new CryptoStream(ms, t, CryptoStreamMode.Write))
                    {
                        try
                        {
                            cs.Write(encryptedData, headerSize, encryptedData.Length - headerSize);
                            cs.Close();
                        }
                        catch
                        {
                            // whatever, we keep going
                        }
                    }
                    decdata = ms.ToArray();
                }

                byte[] digest = hash.ComputeHash(decdata);
                valid4 = true;
                for (int i = 0; i < 32; i++)
                {
                    if (digest[i] != secret[36 + i])
                        valid4 = false;
                }
            }
            finally
            {
                if (key != null)
                {
                    Array.Clear(key, 0, key.Length);
                    key = null;
                }
                if (secret != null)
                {
                    Array.Clear(secret, 0, secret.Length);
                    secret = null;
                }
                if (iv != null)
                {
                    Array.Clear(iv, 0, iv.Length);
                    iv = null;
                }
                aes.Clear();
                hash.Clear();
            }

            // single point of error (also limits timing informations)
            if (!valid1 || !valid2 || !valid3 || !valid4)
            {
                if (decdata != null)
                {
                    Array.Clear(decdata, 0, decdata.Length);
                    decdata = null;
                }
                throw new CryptographicException("Invalid data.");
            }
            return decdata;
        }

        // private stuff

        private static RSA appstartup;
        private readonly static object appstartup_lock = new object();

        private static RSA GetKey()
        {
            if (appstartup == null)
            {
                lock (appstartup_lock)
                {
                    appstartup = new RSACryptoServiceProvider(1536);
                }
            }

            return appstartup;
        }
    }
}
