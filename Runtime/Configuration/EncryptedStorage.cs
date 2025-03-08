using System.Collections.Generic;
using UnityEngine;
using static System.Text.Encoding;
using Convert = System.Convert;

namespace Idem.Configuration
{
    public class EncryptedStorage
    {
        // random bytes, length 64
        private static readonly byte[] _salt = Convert.FromBase64String(
            "coPFd/MilfjoVftiayqmaXuvHRQQ91z6hAptXftkiC9UawA+Zp9qjw5LXB2P7pfy4m2kFbbNyXsny/PptTf9gw==");

        private readonly byte[] _key;
        private readonly Dictionary<string, byte[]> _storage = new();

        public EncryptedStorage(byte[] key)
        {
            _key = key;
        }

        public EncryptedStorage(string base64key) : this(Convert.FromBase64String(base64key))
        {
        }

        public static byte[] Salt(byte[] data)
        {
            return ApplyKey(data, _salt);
        }

        public static string Salt(string data)
        {
            return Convert.ToBase64String(Salt(UTF8.GetBytes(data)));
        }

        public static string Unsalt(string salted)
        {
            return UTF8.GetString(Salt(Convert.FromBase64String(salted)));
        }

        public void Set(string key, string value)
        {
            var encryptedKey = EncryptToBase64(key);
            _storage[encryptedKey] = Encrypt(value);
        }

        public string Get(string key)
        {
            var encryptedKey = EncryptToBase64(key);
            return Decrypt(_storage[encryptedKey]);
        }

        private string EncryptToBase64(string value)
        {
            var bytes = Encrypt(value);
            return Convert.ToBase64String(bytes);
        }

        private byte[] Encrypt(string value)
        {
            var data = UTF8.GetBytes(value);
            return ApplySaltedKey(data, _key, _salt);
        }

        private string Decrypt(byte[] value)
        {
            return UTF8.GetString(
                ApplySaltedKey(value, _key, _salt)
            );
        }

        private static byte[] ApplySaltedKey(byte[] data, byte[] key, byte[] salt)
        {
            var result = new byte[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
                result[i] ^= salt[i % salt.Length];
            }

            return result;
        }

        private static byte[] ApplyKey(byte[] data, byte[] key)
        {
            var result = new byte[data.Length];
            for (var i = 0; i < data.Length; i++) result[i] = (byte)(data[i] ^ key[i % key.Length]);

            return result;
        }

#if UNITY_EDITOR
        public void Dump()
        {
            foreach (var (key, value) in _storage)
            {
                var decryptedKey = Decrypt(Convert.FromBase64String(key));
                Debug.Log($"{decryptedKey}: {Decrypt(value)}");
            }
        }
#endif
    }
}