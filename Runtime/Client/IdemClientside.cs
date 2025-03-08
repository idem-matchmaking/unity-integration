using System;
using Idem.Configuration;
using Idem.Tools;
using Idem.websocket_sharp;
using UnityEngine;

namespace Idem.Client
{
    public class IdemClientside : IIdemClient
    {
        public enum EState
        {
            None = 0,
            Disconnected = 1,
            Connecting = 2,
            Connected = 3,
            RequeueRequired = 4,
            MatchmakingRequested = 5,
            MatchmakingConfirmed = 6,
            MatchFound = 7,
            JoinInfoReceived = 8
        }

        private readonly IClientAuthProvider _authProvider;

        private readonly IdemConfig _config;
        private readonly EncryptedStorage _credentials;
        private (string gameId, string[] servers)? _mmRequest;

        private WebSocket _ws;

        public IdemClientside(IdemConfig config, EncryptedStorage credentials, IClientAuthProvider authProvider)
        {
            _config = config;
            _credentials = credentials;
            _authProvider = authProvider;
            SetState(EState.None);
        }

        public event Action<EState> OnStateChanged;
        public event Action<SuggestedMatch> OnMatchFound;
        public event Action<BaseJoinInfoPayload> OnJoinInfoReceived;
        public EState State { get; private set; } = EState.None;
        public SuggestedMatch? Match { get; private set; }
        public BaseJoinInfoPayload JoinInfo { get; private set; }

        public void FindMatch(string gameId, string[] servers)
        {
            if (State < EState.Connected)
            {
                _mmRequest = (gameId, servers);
                if (State < EState.Connecting)
                    Connect();
                return;
            }

            if (SendThroughWs(new AddPlayerMessage(gameId, _authProvider.GetPlayerId(), servers)))
                SetState(EState.MatchmakingRequested);
        }

        public void StopMatchmaking()
        {
            _mmRequest = null;
            if (_ws == null)
                return;

            _ws.Close();
        }

        public void Connect()
        {
            if (State > EState.Disconnected)
            {
                Debug.LogError($"[Idem][CLIENT] Wrong state for connection: {State}");
                return;
            }

            var playerId = _authProvider.GetPlayerId();
            var joinCode = _credentials.Get(nameof(IdemCredentials.JoinCode));
            var authStr = _authProvider.GetAuthString();
            var url = _config.FullClientUrl(playerId, joinCode, authStr);

            if (_config.debugLogging)
                Debug.Log($"[Idem][CLIENT] Starting Idem connection to: {url}");

            SetState(EState.Connecting);
            _ws = new WebSocket(url);

            _ws.OnOpen += OnWsOpen;
            _ws.OnClose += OnWsClose;
            _ws.OnMessage += OnWsMessage;
            _ws.OnError += OnWsError;

            _ws.ConnectAsync();
        }

        private void SetState(EState newState)
        {
            if (State == newState)
                return;

            State = newState;
            if (_config.debugLogging)
                Debug.Log($"[Idem][CLIENT] State changed to: {State}");

            if (_mmRequest != null && State == EState.Connected)
                FindMatch(_mmRequest.Value.gameId, _mmRequest.Value.servers);

            if (State < EState.Connected)
                Match = null;
            if (State == EState.RequeueRequired)
            {
                Match = null;
                JoinInfo = null;

                var mmRequest = _mmRequest;
                StopMatchmaking(); // Idem breaks connection on requeue
                if (mmRequest.HasValue)
                    FindMatch(mmRequest.Value.gameId, mmRequest.Value.servers);
            }

            OnStateChanged?.Invoke(State);
        }

        private bool SendThroughWs(object payload)
        {
            if (payload == null)
            {
                Debug.LogError("[CLIENT] Trying to send null payload to Idem WS");
                return false;
            }

            if (State != EState.Connected && State != EState.RequeueRequired)
            {
                Debug.LogError($"[CLIENT] Trying to send payload with Idem WS in the wrong state '{State}'");
                return false;
            }

            var json = payload.ToJson();
            if (_config.debugLogging)
                Debug.Log($"[Idem][CLIENT] Sending message to Idem: {json}");

            if (_ws is not { ReadyState: WebSocketState.Open })
            {
                Debug.LogError($"[CLIENT] Trying to send payload with Idem WS in the wrong state '{_ws?.ReadyState}'");
                return false;
            }

            _ws.Send(json);

            return true;
        }

        private void OnWsOpen(object sender, EventArgs param)
        {
            if (_config.debugLogging)
                Debug.Log("[Idem][CLIENT] WS is open");

            SetState(EState.Connected);
        }

        private void OnWsError(object sender, ErrorEventArgs e)
        {
            Debug.Log($"[Idem][CLIENT] WS error: {e.Message}\n{e.Exception}");
            if (State == EState.Connecting)
            {
                SetState(EState.Disconnected);
                _ws.Close();
                _ws = null;
            }
        }

        private void OnWsMessage(object sender, MessageEventArgs e)
        {
            if (_config.debugLogging)
                Debug.Log($"[Idem][CLIENT] WS message: {e.Data}");

            var message = BaseIdemMessage.Parse(e.Data);
            if (message == null)
            {
                Debug.LogError($"[Idem][CLIENT] Failed to parse message: {e.Data}");
                return;
            }

            switch (message)
            {
                case AddPlayerResponseMessage addPlayerResponse:
                    if (State == EState.MatchmakingRequested)
                    {
                        SetState(EState.MatchmakingConfirmed);
                        Debug.Log($"[Idem][CLIENT] Matchmaking confirmed: {addPlayerResponse.payload.gameId}");
                    }

                    break;
                case MatchFoundMessage matchFound:
                    Match = new SuggestedMatch(matchFound.payload);
                    SetState(EState.MatchFound);
                    OnMatchFound?.Invoke(Match.Value);
                    break;
                case IRawJoinInfoProvider provider:
                    JoinInfo = provider.JoinInfo;
                    OnJoinInfoReceived?.Invoke(JoinInfo);
                    SetState(EState.JoinInfoReceived);
                    _mmRequest = null;
                    break;
                case RequeueRequiredMessage requeueReq:
                    Debug.LogWarning($"[Idem][CLIENT] Requeue required: {requeueReq.payload.reason}");
                    SetState(EState.RequeueRequired);
                    break;
                default:
                    if (!message.IsKeepAlive)
                        Debug.LogWarning($"[Idem][CLIENT] Unhandled message: {message}");
                    break;
            }
        }

        private void OnWsClose(object sender, CloseEventArgs e)
        {
            if (_config.debugLogging)
                Debug.Log("[Idem][CLIENT] WS is closed");
            SetState(EState.Disconnected);
            _ws = null;
        }
    }
}