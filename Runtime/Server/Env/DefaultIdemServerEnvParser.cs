using System;
using System.Linq;
using Idem.Tools;
using UnityEngine;

namespace Idem.Server.Env
{
    public class DefaultIdemServerEnvParser : BaseIdemServerEnvParser
    {
        private const string GameIdVar = "idemGameId";
        private const string MatchIdVar = "idemMatchUuid";
        private const string TeamsVar = "idemTeams";

        private string _gameId;
        private string _matchId;
        private PlayerRating[][] _parsedTeams;
        public override string GameId => _gameId;
        public override string MatchId => _matchId;
        public override PlayerRating[][] Teams => _parsedTeams;

        public override void ParseEnv()
        {
            _gameId = Environment.GetEnvironmentVariable(GameIdVar);
            _matchId = Environment.GetEnvironmentVariable(MatchIdVar);
            var teams = Environment.GetEnvironmentVariable(TeamsVar);
            if (!JsonUtil.TryParse(teams, out IdemPlayerRating[][] result))
            {
                Debug.LogError(
                    $"[Idem] [SERVER] Could not parse teams from environment variable '{TeamsVar}': '{teams}'");
                return;
            }

            _parsedTeams = result.Select(arr => arr.Select(r => new PlayerRating(r)).ToArray()).ToArray();
        }
    }
}