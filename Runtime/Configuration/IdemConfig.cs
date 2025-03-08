using System;

namespace Idem.Configuration
{
    [Serializable]
    public partial class IdemConfig
    {
        public const string MainConnectionUrl = "wss://ws-int.idem.gg";
        public const string BetaConnectionUrl = "wss://ws.beta.idem.gg";
        public const string MainIdemClientId = "3b7bo4gjuqsjuer6eatjsgo58u";
        public const string BetaIdemClientId = "3ns1sc0lkrdqh25qvrqb9k3a80";
        public const string ClientUrlTemplate = "{0}/?playerId={1}&code={2}&authorization={3}";

        public string gameId = "1v1";
        public EServerType serverType;
        public string customUrl;
        public string customClientId;
        public bool quitOnCritError = true;
        public bool quitAfterResultReporting = true;
        public bool autoRestartServerIdemConnect = true;
        public int maxIdemConnectAttempts = 10;
        public bool debugLogging;

        public string ConnectionUrl => serverType switch
        {
            EServerType.Main => MainConnectionUrl,
            EServerType.Beta => BetaConnectionUrl,
            EServerType.Custom => customUrl,
            _ => throw new ArgumentOutOfRangeException()
        };

        public string ClientId => serverType switch
        {
            EServerType.Main => MainIdemClientId,
            EServerType.Beta => BetaIdemClientId,
            EServerType.Custom => customClientId,
            _ => throw new ArgumentOutOfRangeException()
        };

        public string FullClientUrl(string playerId, string joinCode, string authorization)
        {
            return string.Format(ClientUrlTemplate, ConnectionUrl, playerId, joinCode, authorization);
        }
    }
}