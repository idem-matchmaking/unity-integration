using System;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Idem.Server.Env
{
    public abstract class BaseIdemServerEnvParser : IIdemEnvironment
    {
        public bool IsValid => !string.IsNullOrWhiteSpace(GameId) && !string.IsNullOrWhiteSpace(MatchId) &&
                               Teams.Length > 0 && Teams.All(t => t.Length > 0);

        public abstract string GameId { get; }
        public abstract string MatchId { get; }
        public abstract PlayerRating[][] Teams { get; }

        public abstract void ParseEnv();

        public virtual void FullEnvDump()
        {
            var dict = Environment.GetEnvironmentVariables();
            var dump = new StringBuilder("[Idem][SERVER] Full environment dump:\n");
            foreach (var key in dict.Keys) dump.Append($"{key}={dict[key]}\n");
            Debug.Log(dump);
        }

        public override string ToString()
        {
            return $"Env: game id '{GameId}', " +
                   $"match id '{MatchId}', " +
                   "teams: " + string.Join(", ",
                       Teams
                           .SelectMany((t, i) =>
                               t.Select(p => $"[{i}] {p.playerId}: {p.rating}")
                           )
                   );
        }
    }
}