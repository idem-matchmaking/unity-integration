using System;
using System.IO;
using Idem.Configuration;
using UnityEditor;

namespace Idem.Editor
{
    public static class ConfigGenerator
    {
        private const string TargetFile = "IdemGen.cs";

        private const string Indent = "\t\t\t";

        private const string Template = @"using UnityEngine;

namespace Idem.Configuration
{{
    public class IdemGen
    {{
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {{
            var storage = new EncryptedStorage({0});
#if UNITY_SERVER
{1}
#endif
{2}
            IdemRuntime.Storage = storage;
        }}

        private static string Cleanup(string saltedBase64) => EncryptedStorage.Unsalt(saltedBase64);
    }}
}}
";

        public static void Generate(string folder, string joinCode, string name, string password)
        {
            var key = new byte[64];
            new Random().NextBytes(key);
            var keyString = $"\"{Convert.ToBase64String(key)}\"";

            var saltedJoinCodeName = EncryptedStorage.Salt(nameof(IdemCredentials.JoinCode));
            var saltedJoinCode = EncryptedStorage.Salt(joinCode);
            var saltedNameName = EncryptedStorage.Salt(nameof(IdemCredentials.UserName));
            var saltedName = EncryptedStorage.Salt(name);
            var saltedPasswordName = EncryptedStorage.Salt(nameof(IdemCredentials.Password));
            var saltedPassword = EncryptedStorage.Salt(password);

            var joinCodeLine =
                $"{Indent}storage.Set(Cleanup(\"{saltedJoinCodeName}\"), Cleanup(\"{saltedJoinCode}\"));";
            var nameLine = $"{Indent}storage.Set(Cleanup(\"{saltedNameName}\"), Cleanup(\"{saltedName}\"));";
            var passwordLine =
                $"{Indent}storage.Set(Cleanup(\"{saltedPasswordName}\"), Cleanup(\"{saltedPassword}\"));";

            var content = string.Format(Template, keyString, $"{nameLine}\n{passwordLine}", joinCodeLine);
            var fullPath = Path.Combine(folder, TargetFile);
            File.WriteAllText(fullPath, content);
            AssetDatabase.ImportAsset(fullPath);
        }
    }
}