using System.Collections.Generic;
using System.Linq;
using Idem.Tools;

namespace Idem
{
    public class BaseIdemMessage
    {
        private const string KeepAliveAction = "keepAlive";

        public string action;
        public IdemError error;
        public string messageId;

        public bool IsKeepAlive => action == KeepAliveAction;

        public static BaseIdemMessage Parse(string fullJson)
        {
            var header = JsonUtil.Parse<BaseIdemMessage>(fullJson);
            return ParseByAction(header, fullJson);
        }

        public static BaseIdemMessage ParseByAction(BaseIdemMessage header, string fullJson)
        {
            return header?.action switch
            {
                "addPlayerResponse" => JsonUtil.Parse<AddPlayerResponseMessage>(fullJson),
                "removePlayerResponse" => JsonUtil.Parse<RemovePlayerResponseMessage>(fullJson),
                "getPlayersResponse" => JsonUtil.Parse<GetPlayersResponseMessage>(fullJson),
                "getMatchesResponse" => JsonUtil.Parse<GetMatchesResponseMessage>(fullJson),
                "updateMatchConfirmedResponse" => JsonUtil.Parse<ConfirmMatchResponseMessage>(fullJson),
                "updateMatchFailedResponse" => JsonUtil.Parse<FailMatchResponseMessage>(fullJson),
                "updateMatchCompletedResponse" => JsonUtil.Parse<CompleteMatchResponseMessage>(fullJson),
                "matchSuggestion" => JsonUtil.Parse<MatchSuggestionMessage>(fullJson),
                "subscribeResponse" => JsonUtil.Parse<SubscribeResponseMessage>(fullJson),
                "joinInfo" => ParseJoinInfo(fullJson),
                "matchFound" => JsonUtil.Parse<MatchFoundMessage>(fullJson),
                "requeueRequired" => JsonUtil.Parse<RequeueRequiredMessage>(fullJson),
                KeepAliveAction => header,
                _ => null
            };
        }

        public static BaseIdemMessage ParseJoinInfo(string fullJson)
        {
            var header = JsonUtil.Parse<BaseJoinInfoMessage>(fullJson);
            return header?.payload.providerName switch
            {
                "hathora" => JsonUtil.Parse<HathoraJoinInfoMessage>(fullJson),
                "i3d" => JsonUtil.Parse<I3DJoinInfoMessage>(fullJson),
                "edgegap" => JsonUtil.Parse<EdgegapJoinInfoMessage>(fullJson),
                "gameye" => JsonUtil.Parse<GameyeJoinInfoMessage>(fullJson),
                _ => header
            };
        }
    }

    public class IdemError
    {
        public int code;
        public string message;
    }

    public class AddPlayerMessage : BaseIdemMessage
    {
        public AddPlayerPayload payload;

        public AddPlayerMessage()
        {
            action = "addPlayer";
        }

        public AddPlayerMessage(string gameId, string playerId, string[] servers) : this()
        {
            payload = new AddPlayerPayload
            {
                gameId = gameId,
                players = new[]
                {
                    new AddPlayerPlayer
                    {
                        playerId = playerId,
                        servers = servers
                    }
                }
            };
        }
    }

    public class AddPlayerResponseMessage : BaseIdemMessage
    {
        public AddPlayerResponsePayload payload;
    }

    public class RemovePlayerMessage : BaseIdemMessage
    {
        public RemovePlayerPayload payload;

        public RemovePlayerMessage()
        {
            action = "removePlayer";
        }

        public RemovePlayerMessage(string gameId, string playerId) : this()
        {
            payload = new RemovePlayerPayload
            {
                gameId = gameId,
                playerId = playerId
            };
        }
    }

    public class RemovePlayerResponseMessage : BaseIdemMessage
    {
        public RemovePlayerResponsePayload payload;
    }

    public class GetPlayersMessage : BaseIdemMessage
    {
        public GameIdPayload payload;

        public GetPlayersMessage()
        {
            action = "getPlayers";
        }

        public GetPlayersMessage(string gameId) : this()
        {
            payload = new GameIdPayload
            {
                gameId = gameId
            };
        }
    }

    public class GetPlayersResponseMessage : BaseIdemMessage
    {
        public GetPlayersResponsePayload payload;
    }

    public class GetMatchesMessage : BaseIdemMessage
    {
        public GameIdPayload payload;

        public GetMatchesMessage()
        {
            action = "getMatches";
        }

        public GetMatchesMessage(string gameId) : this()
        {
            payload = new GameIdPayload
            {
                gameId = gameId
            };
        }
    }

    public class GetMatchesResponseMessage : BaseIdemMessage
    {
        public GetMatchesResponsePayload payload;
    }

    public class ConfirmMatchMessage : BaseIdemMessage
    {
        public MatchIdPayload payload;

        public ConfirmMatchMessage()
        {
            action = "updateMatchConfirmed";
        }

        public ConfirmMatchMessage(string gameId, string matchId) : this()
        {
            payload = new MatchIdPayload
            {
                gameId = gameId,
                matchId = matchId
            };
        }
    }

    public class ConfirmMatchResponseMessage : BaseIdemMessage
    {
        public MatchIdPayload payload;
    }

    public class FailMatchMessage : BaseIdemMessage
    {
        public FailMatchPayload payload;

        public FailMatchMessage()
        {
            action = "updateMatchFailed";
        }

        public FailMatchMessage(string gameId, string matchId, string failedPlayerId,
            IEnumerable<string> allPlayers) : this()
        {
            payload = new FailMatchPayload
            {
                gameId = gameId,
                matchId = matchId,
                remove = new[] { failedPlayerId },
                requeue = allPlayers.Where(p => p != failedPlayerId).ToArray()
            };
        }

        public FailMatchMessage(string gameId, string matchId, string[] removePlayers, string[] requeuePlayers) : this()
        {
            payload = new FailMatchPayload
            {
                gameId = gameId,
                matchId = matchId,
                remove = removePlayers,
                requeue = requeuePlayers
            };
        }
    }

    public class FailMatchResponseMessage : BaseIdemMessage
    {
        public FailMatchResponsePayload payload;
    }

    public class CompleteMatchMessage : BaseIdemMessage
    {
        public CompleteMatchPayload payload;

        public CompleteMatchMessage()
        {
            action = "updateMatchCompleted";
        }

        public CompleteMatchMessage(IdemMatchResult result) : this()
        {
            payload = new CompleteMatchPayload
            {
                gameId = result.gameId,
                matchId = result.matchId,
                server = result.server,
                gameLength = result.gameLength,
                teams = result.teams.Select(t => new TeamResult
                {
                    rank = t.rank,
                    players = t.players.Select(p => new PlayerResult
                    {
                        playerId = p.playerId,
                        score = p.score
                    }).ToArray()
                }).ToArray()
            };
        }
    }

    public class CompleteMatchResponseMessage : BaseIdemMessage
    {
        public CompleteMatchReponsePayload payload;
    }

    public class MatchSuggestionMessage : BaseIdemMessage
    {
        public MatchSuggestionPayload payload;
    }

    public class SubscribeMessage : BaseIdemMessage
    {
        public SubscribePayload payload;

        public SubscribeMessage()
        {
            action = "subscribe";
        }

        public SubscribeMessage(string[] gameIds, int priority, int rateLimit) : this()
        {
            payload = new SubscribePayload
            {
                gameIds = gameIds,
                priority = priority,
                rateLimit = rateLimit
            };
        }
    }

    public class RequeueRequiredMessage : BaseIdemMessage
    {
        public RequeueRequiredPayload payload;
    }

    public class MatchFoundMessage : BaseIdemMessage
    {
        public MatchFoundPayload payload;
    }

    public class SubscribeResponseMessage : BaseIdemMessage
    {
        public SubscribePayload payload;
    }

    public class BaseJoinInfoMessage : BaseIdemMessage
    {
        public BaseJoinInfoPayload payload;
    }

    public interface IRawJoinInfoProvider
    {
        BaseJoinInfoPayload JoinInfo { get; }
    }

    public class HathoraJoinInfoMessage : BaseIdemMessage, IRawJoinInfoProvider
    {
        public HathoraJoinInfoPayload payload;
        public BaseJoinInfoPayload JoinInfo => payload;
    }

    public class EdgegapJoinInfoMessage : BaseIdemMessage, IRawJoinInfoProvider
    {
        public EdgegapJoinInfoPayload payload;
        public BaseJoinInfoPayload JoinInfo => payload;
    }

    public class I3DJoinInfoMessage : BaseIdemMessage, IRawJoinInfoProvider
    {
        public I3DJoinInfoPayload payload;
        public BaseJoinInfoPayload JoinInfo => payload;
    }

    public class GameyeJoinInfoMessage : BaseIdemMessage, IRawJoinInfoProvider
    {
        public GameyeJoinInfoPayload payload;
        public BaseJoinInfoPayload JoinInfo => payload;
    }

    public class AddPlayerPayload
    {
        public string gameId;
        public string partyName;
        public AddPlayerPlayer[] players;
    }

    public class AddPlayerResponsePayload
    {
        public string gameId;
        public Player[] players;
    }

    public class RemovePlayerPayload
    {
        public string gameId;
        public string playerId;
    }

    public class RemovePlayerResponsePayload
    {
        public string gameId;
        public string playerId;
        public string reference;
    }

    public class GameIdPayload
    {
        public string gameId;
    }

    public class GetPlayersResponsePayload
    {
        public string gameId;
        public PlayerStatus[] players;
    }

    public class GetMatchesResponsePayload
    {
        public string gameId;
        public Match[] matches;
    }

    public class MatchIdPayload
    {
        public string gameId;
        public string matchId;
    }

    public class CompleteMatchPayload : MatchIdPayload
    {
        public float gameLength;
        public string server;
        public TeamResult[] teams;
    }

    public class FailMatchPayload : MatchIdPayload
    {
        public string[] remove;
        public string[] requeue;
    }

    public class FailMatchResponsePayload : MatchIdPayload
    {
        public Player[] removed;
        public Player[] requeued;
    }

    public class CompleteMatchReponsePayload : MatchIdPayload
    {
        public PlayerFullStats[] players;
    }

    public class MatchSuggestionPayload
    {
        public string gameId;
        public Match match;
    }

    public class RequeueRequiredPayload
    {
        public string gameId;
        public string reason;
    }

    public class MatchFoundPayload
    {
        public string gameId;
        public string matchUuid;
    }

    public class SubscribePayload
    {
        public string[] gameIds;
        public int priority;
        public int rateLimit;
    }

    public class BaseJoinInfoPayload
    {
        public ConnectionInfo connectionInfo;
        public string providerName;
        public string providerReference;
    }

    public class HathoraJoinInfoPayload : BaseJoinInfoPayload
    {
        public HathoraProviderInformation rawProviderInformation;
    }

    public class HathoraProviderInformation
    {
        public HathoraPort[] additionalExposedPorts;
        public HathoraPort exposedPort;
        public string roomId;
        public string status;
    }

    public class HathoraPort
    {
        public string host;
        public string name;
        public int port;
        public string transportType;
    }

    public class EdgegapJoinInfoPayload : BaseJoinInfoPayload
    {
        public EdgegapProviderInformation rawProviderInformation;
    }

    public class EdgegapProviderInformation
    {
        public string fqdn;
        public string last_status;
        public EdgegapLocation location;
        public Dictionary<string, EdgegapPort> ports;
        public string public_ip;
        public string request_id;
        public string[] tags;
    }

    public class EdgegapPort
    {
        public int external;
        public int @internal;
        public string link;
        public string name;
        public string protocol;
        public int proxy;
        public bool tls_upgrade;
    }

    public class EdgegapLocation
    {
        public string administrative_division;
        public string city;
        public string continent;
        public string country;
        public string timezone;
    }

    public class I3DJoinInfoPayload : BaseJoinInfoPayload
    {
        public I3DProviderInformation rawProviderInformation;
    }

    public class I3DProviderInformation
    {
        public string id;
        public I3DIpAddress[] ipAddress;
        public I3DProperty[] properties;
        public int status;
    }

    public class I3DIpAddress
    {
        public string ipAddress;
        public int ipVersion;
        public int @private;
    }

    public class I3DProperty
    {
        public string id;
        public string propertyKey;
        public int propertyType;
        public string propertyValue;
    }

    public class GameyeJoinInfoPayload : BaseJoinInfoPayload
    {
        public GameyeProviderInformation rawProviderInformation;
    }

    public class GameyeProviderInformation
    {
        public string host;
        public GameyePort[] ports;
    }

    public class GameyePort
    {
        public int container;
        public int host;
        public string type;
    }

    public class ConnectionInfo
    {
        public string domain;
        public string host;
        public string ip;
        public ConnectionPort[] ports;
    }

    public class ConnectionPort
    {
        public string name;
        public int port;
        public string protocol;
    }

    public class AddPlayerPlayer : Player
    {
        public string[] servers;
    }

    public class TeamResult
    {
        public PlayerResult[] players;
        public int rank;
    }

    public class PlayerResult
    {
        public string playerId;
        public float score;
    }

    public class Match
    {
        public string server;
        public Team[] teams;
        public string uuid;
    }

    public class Team
    {
        public Player[] players;
    }

    public class Player
    {
        public string playerId;
        public string reference;
    }

    public class PlayerStatus : Player
    {
        public string state;
    }

    public class PlayerFullStats : Player
    {
        public int losses;
        public int matchesPlayed;
        public float rankingDeltaLastGame;
        public float rankingPoints;
        public float rating;
        public float ratingDeltaLastGame;
        public float ratingUncertainty;
        public string season;
        public int seasonLosses;
        public int seasonMatchesPlayed;
        public int seasonWins;
        public int totalLosses;
        public int totalMatchesPlayed;
        public int totalWins;
        public float winRatio;
        public int wins;
    }
}