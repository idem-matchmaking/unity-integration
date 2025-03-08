using Idem.Client;
using UnityEngine;

namespace Idem.Sample
{
    public class PackageUsageClient : MonoBehaviour, IClientAuthProvider
    {
        private string _playerId;

        private void Start()
        {
            IdemRuntime.InitClient(this);
            IdemRuntime.Client.OnStateChanged += state => { Debug.Log($"[Idem] EVENT State changed: {state}"); };
            IdemRuntime.Client.OnMatchFound += match => { Debug.Log($"[Idem] EVENT Match found: {match.Uuid}"); };
            IdemRuntime.Client.OnJoinInfoReceived += joinInfo =>
            {
                Debug.Log($"[Idem] EVENT Join info received: {joinInfo.providerName}");
            };
            IdemRuntime.Client.FindMatch("1v1", new[] { "Frankfurt" });
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(100, 100, 100, 100), "Start MM"))
                IdemRuntime.Client.FindMatch("1v1", new[] { "Frankfurt" });
            if (GUI.Button(new Rect(100, 200, 100, 100), "Stop MM")) IdemRuntime.Client.StopMatchmaking();
        }

        public string GetPlayerId()
        {
            return _playerId ??= $"player_{Random.Range(0, 100000)}";
        }
    }
}