using System;
using System.Linq;
using Idem.Tools;

namespace Idem.Server.Env
{
    public class HathoraIdemServerEnvParser : BaseIdemServerEnvParser
    {
        private const string HathoraRoot = "HATHORA_INITIAL_ROOM_CONFIG";

        private string _gameId;
        private string _matchId;
        private PlayerRating[][] _parsedTeams;
        public override string GameId => _gameId;
        public override string MatchId => _matchId;
        public override PlayerRating[][] Teams => _parsedTeams;

        public override void ParseEnv()
        {
            var hathoraRaw = Environment.GetEnvironmentVariable(HathoraRoot);
            var hathoraParsed = hathoraRaw.FromJson<HathoraEnv>();
            _gameId = hathoraParsed.idemGameId;
            _matchId = hathoraParsed.idemMatchUuid;
            _parsedTeams = hathoraParsed.idemTeams.Select(arr => arr.Select(r => new PlayerRating(r)).ToArray())
                .ToArray();
        }

        private class HathoraEnv
        {
            public string idemGameId;
            public string idemMatchUuid;
            public IdemPlayerRating[][] idemTeams;
        }
    }
}