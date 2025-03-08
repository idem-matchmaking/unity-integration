namespace Idem.Client
{
    public readonly struct SuggestedMatch
    {
        public readonly string GameId;
        public readonly string Uuid;

        public SuggestedMatch(string gameId, string uuid, string server, SuggestedTeam[] teams)
        {
            GameId = gameId;
            Uuid = uuid;
        }

        public SuggestedMatch(MatchFoundPayload payload)
        {
            GameId = payload.gameId;
            Uuid = payload.matchUuid;
        }
    }

    public readonly struct SuggestedTeam
    {
        public readonly SuggestedPlayer[] Players;

        public SuggestedTeam(SuggestedPlayer[] players)
        {
            Players = players;
        }
    }

    public readonly struct SuggestedPlayer
    {
        public readonly string PlayerId;
        public readonly string Reference;

        public SuggestedPlayer(string playerId, string reference)
        {
            PlayerId = playerId;
            Reference = reference;
        }
    }
}