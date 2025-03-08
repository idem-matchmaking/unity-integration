namespace Idem
{
    public class IdemPlayer
    {
        public string playerId;
        public int teamId;

        public IdemPlayer()
        {
        }

        public IdemPlayer(int teamId, string playerId)
        {
            this.teamId = teamId;
            this.playerId = playerId;
        }
    }

    public class IdemMatchResult
    {
        public string gameId;
        public float gameLength;
        public string matchId;
        public string server;
        public IdemTeamResult[] teams;
    }

    public class IdemTeamResult
    {
        public IdemPlayerResult[] players;
        public int rank;
    }

    public class IdemPlayerResult
    {
        public string playerId;
        public float score;

        public IdemPlayerResult()
        {
        }

        public IdemPlayerResult(string playerId, float score)
        {
            this.playerId = playerId;
            this.score = score;
        }
    }

    public class IdemPlayerRating
    {
        public string playerId;
        public float rating;
    }
}