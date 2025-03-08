namespace Idem.Server.Env
{
    public interface IIdemEnvironment
    {
        string GameId { get; }
        string MatchId { get; }
        PlayerRating[][] Teams { get; }
    }
}