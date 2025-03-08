using Idem.Server.Env;

namespace Idem.Server
{
    public interface IIdemServer
    {
        bool IsServerReady { get; }
        IdemServerside.EMatchState MatchState { get; }
        IIdemEnvironment Environment { get; }
        bool ConfirmMatch();
        bool CompleteMatch(float gameLength, string serverName, IdemTeamResult[] results);
        bool FailMatch();
    }
}