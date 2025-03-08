using System;

namespace Idem.Client
{
    public interface IIdemClient
    {
        IdemClientside.EState State { get; }

        SuggestedMatch? Match { get; }
        BaseJoinInfoPayload JoinInfo { get; }
        event Action<IdemClientside.EState> OnStateChanged;
        event Action<SuggestedMatch> OnMatchFound;
        event Action<BaseJoinInfoPayload> OnJoinInfoReceived;

        void FindMatch(string gameId, string[] servers);
        void StopMatchmaking();
    }
}