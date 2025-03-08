using System.Collections;
using System.Linq;
using Idem.Server.Env;
using UnityEngine;

namespace Idem.Sample
{
    public class PackageUsageServer : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(Test());
        }

        private IEnumerator Test()
        {
            Debug.Log("[IDEM] Starting server");
            yield return IdemRuntime.InitServerCoroutine(new HathoraIdemServerEnvParser());
            Debug.Log(
                $"[IDEM] Server started: {IdemRuntime.Server.MatchState}, ready {IdemRuntime.Server.IsServerReady}");

            yield return new WaitForSeconds(5);
            Debug.Log("[IDEM] Confirming match");
            IdemRuntime.Server.ConfirmMatch();

            yield return new WaitForSeconds(20);
            Debug.Log("[IDEM] Completing match");
            var counter = 0;
            IdemRuntime.Server.CompleteMatch(20, "mainServer", IdemRuntime.Server.Environment.Teams
                .Select(t => new IdemTeamResult
                {
                    rank = counter++,
                    players = t.Select(p => new IdemPlayerResult(p.playerId, Random.Range(0, 100))).ToArray()
                })
                .ToArray());
        }
    }
}