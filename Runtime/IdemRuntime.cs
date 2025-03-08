using System.Collections;
using System.Threading.Tasks;
using Idem.Client;
using Idem.Configuration;
using Idem.Server;
using Idem.Server.Env;
using UnityEngine;

namespace Idem
{
    public static class IdemRuntime
    {
        private static IdemConfigProvider configProvider = IdemConfigProvider.Default;
        private static IdemConfig configValue;

        private static IdemClientside _client;
        private static IdemServerside _server;
        public static EncryptedStorage Storage { private get; set; }
        public static IIdemClient Client => _client;
        public static IIdemServer Server => _server;

        private static IdemConfig Config => configValue ??= configProvider.GetConfig();

        public static void SetConfigProvider(IdemConfigProvider provider)
        {
            if (_client != null || _server != null)
            {
                Debug.LogError("[Idem] Cannot change config provider after initialization");
                return;
            }

            configValue = null;
            configProvider = provider;
        }

        public static void InitClient(IClientAuthProvider authProvider)
        {
            if (Storage == null)
            {
                Debug.LogError($"[Idem] Idem credentials have not been set and applied in the configuration. Halting.");
                return;
            }
            _client = new IdemClientside(Config, Storage, authProvider);
            _client.Connect();
        }

        public static Task InitServer(BaseIdemServerEnvParser envParser = null)
        {
            if (Storage == null)
            {
                Debug.LogError($"[Idem] Idem credentials have not been set and applied in the configuration. Halting.");
                return Task.CompletedTask;
            }
            
            _server = new IdemServerside(Config, Storage, envParser);
            return _server.Start();
        }

        public static IEnumerator InitServerCoroutine(BaseIdemServerEnvParser envParser = null)
        {
            yield return InitServer(envParser);
        }

        public static void StopClient()
        {
            _client.StopMatchmaking();
            _client = null;
        }

        public static void StopServer()
        {
            _server.Stop();
            _server = null;
        }

        public static void Halt()
        {
            StopClient();
            StopServer();
        }

#if UNITY_EDITOR
        public static void DumpStorage()
        {
            Storage.Dump();
        }
#endif
    }
}