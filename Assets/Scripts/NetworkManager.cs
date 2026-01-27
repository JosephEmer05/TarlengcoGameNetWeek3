using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    #region Public Variables
    [Header("Spawning")]
    [SerializeField] private NetworkPrefabRef playerPrefab;

    [Header("UI References")]
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Dropdown colorDropdown;
    [SerializeField] private GameObject uiPanel;
    #endregion

    #region Private Variables
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();
    private NetworkRunner _networkRunner;
    #endregion

    public static string LocalPlayerName;
    public static Color LocalPlayerColor;

    public void OnJoinGameClicked()
    {
        LocalPlayerName = nameInput.text;
        if (string.IsNullOrEmpty(LocalPlayerName)) LocalPlayerName = "Player";

        switch (colorDropdown.value)
        {
            case 0: LocalPlayerColor = Color.red; break;
            case 1: LocalPlayerColor = Color.green; break;
            case 2: LocalPlayerColor = Color.blue; break;
            default: LocalPlayerColor = Color.white; break;
        }

        if (uiPanel != null) uiPanel.SetActive(false);
        StartGame();
    }

    async void StartGame()
    {
        _networkRunner = gameObject.AddComponent<NetworkRunner>();
        _networkRunner.ProvideInput = true;

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();

        if (scene.IsValid)
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        await _networkRunner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "TestSession",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            var position = new Vector3(0, 1, 0);
            var networkObject = runner.Spawn(playerPrefab, position, Quaternion.identity, player);
            _spawnedCharacters.Add(player, networkObject);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out var playerObject))
        {
            runner.Despawn(playerObject);
            _spawnedCharacters.Remove(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        data.InputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input.Set(data);
    }

    #region Unused Fusion Callbacks
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    #endregion
}