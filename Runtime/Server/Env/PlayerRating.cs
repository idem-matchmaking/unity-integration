namespace Idem.Server.Env
{
    public readonly struct PlayerRating
    {
        public readonly string playerId;
        public readonly float rating;

        public PlayerRating(IdemPlayerRating r)
        {
            playerId = r.playerId;
            rating = r.rating;
        }
    }
}