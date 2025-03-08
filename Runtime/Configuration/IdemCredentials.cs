using System;
using Idem.Tools;
using UnityEngine;

namespace Idem.Configuration
{
    [Serializable]
    public class IdemCredentials
    {
        public string JoinCode;
        public string UserName;
        public string Password;

        public IdemCredentials()
        {
        }

        public IdemCredentials(string joinCode, string userName, string password)
        {
            JoinCode = joinCode;
            UserName = userName;
            Password = password;
        }

        public void CopyFrom(IdemCredentials other)
        {
            JoinCode = other.JoinCode;
            UserName = other.UserName;
            Password = other.Password;
        }

        public bool Equals(IdemCredentials other)
        {
            return other != null && JoinCode == other.JoinCode && UserName == other.UserName &&
                   Password == other.Password;
        }

        public static IdemCredentials FromJsonOrEmpty(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new IdemCredentials();

            try
            {
                return json.FromJson<IdemCredentials>();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Idem] Could not parse credentials from JSON: {e.Message}");
                return new IdemCredentials();
            }
        }

        public static IdemCredentials FillFrom(EncryptedStorage credentials)
        {
            return new IdemCredentials(
                credentials.Get(nameof(JoinCode)),
                credentials.Get(nameof(UserName)),
                credentials.Get(nameof(Password))
            );
        }
    }
}