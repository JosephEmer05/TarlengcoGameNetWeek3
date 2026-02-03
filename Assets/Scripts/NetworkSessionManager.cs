using Fusion;
using Fusion.Sockets;
using Network;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkSessionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkSessionManager Instance { get; private set; }

    public string gameplaySceneName = "Game";

    private NetworkRunner _networkRunner;
    public List<PlayerRef> _joinedPlayers = new();
    public IReadOnlyList<PlayerRef> JoinedPlayers => _joinedPlayers;

    public event Action<PlayerRef> OnPlayerJoinedEvent;
    public event Action<PlayerRef> OnPlayerLeftEvent;

    public Color localPlayerColor = Color.red;
    public string localPlayerName = "Player";
    public int localTeamID = 1;

    public void SetPlayerName(string name) => localPlayerName = name;
    public void SetTeam(int teamIndex) => localTeamID = teamIndex + 1;
    public void SetColor(int colorIndex)
    {
        switch (colorIndex)
        {
            case 0: localPlayerColor = Color.red; break;
            case 1: localPlayerColor = Color.green; break;
            case 2: localPlayerColor = Color.blue; break;
        }
    }

    public void UI_StartGame()
    {
#if CLIENT
        StartGame(GameMode.Client);
#endif
    }

    async void StartGame(GameMode game)
    {
        _networkRunner = this.gameObject.AddComponent<NetworkRunner>();
        _networkRunner.ProvideInput = true;

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        await _networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = game,
            SessionName = "TestSession",
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            Scene = SceneRef.FromIndex(1)
        });
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
#if SERVER
        StartGame(GameMode.Server);
#endif
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        data.InputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        input.Set(data);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        _joinedPlayers.Add(player);
        OnPlayerJoinedEvent?.Invoke(player);
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        _joinedPlayers.Remove(player);
        OnPlayerLeftEvent?.Invoke(player);
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}