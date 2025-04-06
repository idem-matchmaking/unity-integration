# Idem Unity Integration
[Player-based Idem matchmaking](https://docs.idem.gg/setup-player-based) integration package. Handles Idem API calls for both client and server implementation.

## Installation
### OpenUPM package
[Install from OpenUPM package](https://openupm.com/packages/gg.idem.unity-integration/)
### Unity Package Manager
  * Open Package manager window in Unity
  * Click on the `+` button in the top left corner
  * Select `Add package from git URL`
  * Enter `git+https://github.com/idem-matchmaking/unity-integration.git`
  * Click `Add`
### manifest.json
  * Open `Packages/manifest.json`
  * Add line for the latest version in the `main` branch
``` json
"gg.idem.unity-integration": "git+https://github.com/idem-matchmaking/unity-integration#wip/prototype",
```
  * Or for a specific release `v1.0.0` use
``` json
"gg.idem.unity-integration": "git+https://github.com/idem-matchmaking/unity-integration#v1.0.0",
```

## Configuration
* Follow the [Idem Unity Integration](https://docs.idem.gg/setup-player-based) guide to set up your Idem account
* Go to `Idem -> Configuration` in the Unity Editor
* Fill in `Game mode ID`, [Join code](https://docs.idem.gg/setup-player-based#%F0%9F%84%B2-retrieve-join-code), [User name and Password](https://console.idem.gg/api_users/) from Idem's dashboard
* Press 'Apply config' to bake the values into the build

In order to use multiple configurations in the same project or allow for other config delivery methods, `IdemConfigurationProvider` class can be overridden and provided to `IdemRuntime.SetConfigProvider()` before client/server initialization.

## Usage
### Minimalistic samples
Can be found in `Idem/Sample/` folder. The samples are: `PackageUsageClient.cs` and `PackageUsageServer.cs`.

### Client
* In order to initialize Idem on the client side implementation of `IClientAuthProvider` interface is required.
    ```csharp
    public interface IClientAuthProvider
    {
        public const string SkipAuthorization = "Demo";

        string GetPlayerId();

        string GetAuthString()
        {
            return SkipAuthorization;
        }
    }
    ```
    * `GetPlayerId()` should return the player's unique identifier to be used during matchmaking
    * `GetAuthString()` should return the authorization string, `Demo` (default) for no player authentication on Idem side or one of [third-party auth provider strings](https://docs.idem.gg/category/player-authentication)
* Call `IdemRuntime.InitClient(authProvider)` to initialize Idem client
* After the initialization matchmaking calls become available
  * `IdemRuntime.Client.FindMatch(string gameMode, string[] servers);` starts the matchmaking process
    * `gameMode` the game mode to find a match for, one of the game modes configured for the Idem account
    * `servers` an array of servers appropriate for the match, note that some hosting providers may require specific server name values
  * `IdemRuntime.Client.StopMatchmaking()` stops the matchmaking process
* In order to receive updates on matchmaking process, subscribe to events
  * `IdemRuntime.Client.OnStateChanged` is called on any matchmaking state change with `MatchmakingState` as a parameter
    ```csharp
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
    ```
  * `IdemRuntime.Client.OnMatchFound` is called when a match is found with `SuggestedMatch` as a parameter
    ``` csharp
    public readonly struct SuggestedMatch
    {
        public readonly string GameId;
        public readonly string Uuid;
    }
    ```
  * `IdemRuntime.Client.OnJoinInfoReceived` is called when there is an instance of the server started on a hosting provider of choice, initialized and ready to accept players

### Server
* Before using any Idem calls on the server side, `IdemRuntime.InitServerCoroutine()` or `IdemRuntime.InitServer()` should be called an awaited
  ``` csharp
  yield return IdemRuntime.InitServerCoroutine();
  ```
  * Alternatively one could await until `IdemRuntime.Server.IsServerReady`
  * Some hosting providers may require special environment handling, in this case, provide an instance of `BaseIdemServerEnvParser` to `InitServer` call
  * For [Hathora](https://hathora.dev) `HathoraIdemServerEnvParser` may be used
* After initialization match management calls become available
  * `IdemRuntime.Server.ConfirmMatch()` confirms that match can be started, i.e. all the players joined or some other requirements are met
  * `IdemRuntime.Server.FailMatch()` fails the match either before confirmation or after in case proceeding is deemed impossible
  * `IdemRuntime.Server.CompleteMatch(float gameLength, string serverName, IdemTeamResult[] results)` completes the match, providing the game length, server name and full results for each team and player in the match back to Idem matchmaking
