using System;
using System.Linq;
using System.Threading.Tasks;
using Idem.Configuration;
using Idem.Server.Env;
using Idem.Tools;
using Idem.websocket_sharp;
using UnityEngine;

namespace Idem.Server
{
    public class IdemServerside : IIdemServer
    {
        public enum EMatchState
        {
            None = 0,
            Failed = 1,
            Created = 10,
            Confirmed = 11,
            Completed = 12
        }

        public const int NormalExitCode = 0;
        public const int InvalidEnvironmentExitCode = 1;
        public const int CannotConnectExitCode = 2;
        public const int FailedMatchExitCode = 3;
        public const int CannotAuthorizeExitCode = 4;

        public const int IdemResponseTimeout = 30;

        private readonly IdemConfig _config;
        private readonly EncryptedStorage _credentials;
        private readonly BaseIdemServerEnvParser _envParser;
        private int _connectAttempts;
        private TaskCompletionSource<object> _initCompletion;
        private DateTime _outstandingRequestSentAt = DateTime.MaxValue;
        private int _reconnectDelay;
        private WebSocket _ws;

        public IdemServerside(IdemConfig config, EncryptedStorage credentials, BaseIdemServerEnvParser envParser = null)
        {
            _config = config;
            _credentials = credentials;
            _envParser = envParser ?? new DefaultIdemServerEnvParser();
            _envParser.ParseEnv();

            if (_config.debugLogging)
                _envParser.FullEnvDump();

            if (_envParser.IsValid)
            {
                MatchState = EMatchState.Created;
            }
            else if (_config.quitOnCritError)
            {
                Debug.LogError("[Idem][SERVER] Invalid environment, quitting");
                Application.Quit(InvalidEnvironmentExitCode);
            }
        }

        public bool IsServerReady => _ws is { ReadyState: WebSocketState.Open };
        public EMatchState MatchState { get; private set; } = EMatchState.None;
        public IIdemEnvironment Environment => _envParser;

        public bool ConfirmMatch()
        {
            if (MatchState > EMatchState.Created)
            {
                Debug.LogError($"[Idem][SERVER] Wrong match state for confirmation: {MatchState}");
                return false;
            }

            return SendThroughWs(new ConfirmMatchMessage(_envParser.GameId, _envParser.MatchId));
        }

        public bool CompleteMatch(float gameLength, string serverName, IdemTeamResult[] results)
        {
            if (MatchState != EMatchState.Confirmed)
            {
                Debug.LogError($"[Idem][SERVER] Wrong match state for completion: {MatchState}");
                return false;
            }

            return SendThroughWs(new CompleteMatchMessage(new IdemMatchResult
            {
                gameId = _envParser.GameId,
                gameLength = gameLength,
                matchId = _envParser.MatchId,
                server = serverName,
                teams = results
            }));
        }

        public bool FailMatch()
        {
            if (MatchState != EMatchState.Created)
            {
                Debug.LogError($"[Idem][SERVER] Wrong match state for failing: {MatchState}");
                return false;
            }

            var allPlayers = _envParser.Teams.SelectMany(t => t).Select(p => p.playerId).ToArray();
            return SendThroughWs(new FailMatchMessage(_envParser.GameId, _envParser.MatchId, allPlayers,
                Array.Empty<string>()));
        }

        public async Task Start()
        {
            if (_initCompletion != null)
            {
                await _initCompletion.Task;
                return;
            }

            if (_connectAttempts >= _config.maxIdemConnectAttempts)
            {
                Debug.LogError(
                    $"[Idem][SERVER] Could not connect to Idem after {_config.maxIdemConnectAttempts} attempts, quitting");
                if (_config.quitOnCritError)
                    Application.Quit(CannotConnectExitCode);
                return;
            }

            _connectAttempts++;

            try
            {
                var completion = new TaskCompletionSource<object>();
                _initCompletion = completion;
                var clientId = _config.ClientId;
                var credentials = IdemCredentials.FillFrom(_credentials);
                var token = await AWSAuth.AuthAndGetToken(credentials.UserName, credentials.Password, clientId,
                    _config.debugLogging);

                if (completion != _initCompletion)
                    return;

                if (string.IsNullOrWhiteSpace(token))
                {
                    Debug.LogError("[Idem][SERVER] Could not authorize with AWS Cognito: response token is empty");
                    if (_config.quitOnCritError)
                        Application.Quit(CannotAuthorizeExitCode);

                    MatchState = EMatchState.Failed;
                    _initCompletion.SetResult(null);
                    _initCompletion = null;
                    return;
                }

                var baseUrl = _config.ConnectionUrl;
                var connectionUrl = $"{baseUrl}?authorization={token}";

                if (_config.debugLogging)
                    Debug.Log($"[Idem][SERVER] Starting Idem connection to: {connectionUrl}");

                _ws = new WebSocket(connectionUrl);
                _ws.OnOpen += OnWsOpen;
                _ws.OnClose += OnWsClose;
                _ws.OnMessage += OnWsMessage;
                _ws.OnError += OnWsError;

                _ws.Connect();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Idem][SERVER] Could not start Idem server: {e}");
                if (_config.quitOnCritError)
                    Application.Quit(CannotConnectExitCode);
                _initCompletion?.SetResult(null);
                _initCompletion = null;
            }
        }

        public void Stop()
        {
            if (_initCompletion != null)
                InitCompletion();

            if (_ws != null)
            {
                _ws.Close();
                _ws = null;
            }
        }

        private void InitCompletion()
        {
            _initCompletion?.SetResult(null);
            _initCompletion = null;
        }

        private async void StartWithDelay(float delay)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(delay));
                await Start();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void OnWsOpen(object sender, EventArgs param)
        {
            if (_config.debugLogging)
                Debug.Log("[Idem][SERVER] Idem WS is open");

            InitCompletion();
            _outstandingRequestSentAt = DateTime.MaxValue;
            _connectAttempts = 0;
            _reconnectDelay = 0;
        }

        private bool SendThroughWs(object payload)
        {
            if (_outstandingRequestSentAt != DateTime.MaxValue &&
                (DateTime.UtcNow - _outstandingRequestSentAt).TotalSeconds < IdemResponseTimeout)
            {
                Debug.LogError(
                    "[Idem][SERVER] Trying to send payload with Idem WS while another request is outstanding");
                return false;
            }

            if (payload == null)
            {
                Debug.LogError("[Idem][SERVER] Trying to send null payload to Idem WS");
                return false;
            }

            var json = payload.ToJson();
            if (_config.debugLogging)
                Debug.Log($"[Idem][SERVER] Sending message to Idem: {json}");

            if (_ws is not { ReadyState: WebSocketState.Open })
            {
                Debug.LogError(
                    $"[Idem][SERVER] Trying to send payload with Idem WS in the wrong state '{_ws?.ReadyState}'");
                return false;
            }

            _outstandingRequestSentAt = DateTime.UtcNow;
            _ws.Send(json);

            return true;
        }

        private void OnWsError(object sender, ErrorEventArgs e)
        {
            Debug.Log($"[Idem][SERVER] Idem WS error: {e.Message}\n{e.Exception}");

            if (_initCompletion != null && _ws != null)
            {
                _ws.Close();
                _ws = null;
            }

            if (_config.autoRestartServerIdemConnect)
                StartWithDelay(GetReconnectDelay());
        }

        private void OnWsMessage(object sender, MessageEventArgs e)
        {
            if (_config.debugLogging)
                Debug.Log($"[Idem][SERVER] Idem WS message: {e.Data}");

            _outstandingRequestSentAt = DateTime.MaxValue;
            var message = BaseIdemMessage.Parse(e.Data);
            switch (message)
            {
                case CompleteMatchResponseMessage:
                    MatchState = EMatchState.Completed;
                    if (_config.quitAfterResultReporting)
                    {
                        Debug.Log($"[Idem][SERVER] Quitting after reporting results, response: {e.Data}");
                        Application.Quit(NormalExitCode);
                    }

                    break;
                case FailMatchResponseMessage:
                    Debug.Log($"[Idem][SERVER] Quitting after failing match, response: {e.Data}");
                    MatchState = EMatchState.Failed;
                    Application.Quit(FailedMatchExitCode);
                    break;
                case ConfirmMatchResponseMessage:
                    MatchState = EMatchState.Confirmed;
                    Debug.Log($"[Idem][SERVER] Match confirmed: {e.Data}");
                    break;
            }
        }

        private void OnWsClose(object sender, CloseEventArgs e)
        {
            Debug.Log("[Idem][SERVER] Idem WS is closed");
            _ws = null;

            if (_config.autoRestartServerIdemConnect)
                StartWithDelay(GetReconnectDelay());
        }

        private int GetReconnectDelay()
        {
            var delay = _reconnectDelay;
            _reconnectDelay = _reconnectDelay switch
            {
                0 => 1,
                < 10 => _reconnectDelay * 2,
                < 60 => _reconnectDelay + 5,
                _ => 60
            };
            return delay;
        }
    }
}