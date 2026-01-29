using UnityEngine;
using Fusion;
using System.Collections.Generic;
public class NetworkGameManager : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();
    private List<PlayerRef> _joinedPlayers = new();
  
    private NetworkSessionManager _networkSessionManager;

    private int maxPlayers = 2;
    private int timerBeforeStart = 3;

    private void Awake()
    {
        _networkSessionManager = GetComponent<NetworkSessionManager>();
    }

    public override void Spawned()
    {
        base.Spawned();
        _networkSessionManager.OnPlayerJoinedEvent += OnPlayerJoined;
        _networkSessionManager.OnPlayerLeftEvent += OnPlayerLeft;
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);  
        _networkSessionManager.OnPlayerJoinedEvent -= OnPlayerJoined;
        _networkSessionManager.OnPlayerLeftEvent -= OnPlayerLeft;
    }

    private void OnPlayerJoined(PlayerRef player)
    {
        if (!HasStateAuthority) return;
        if (_networkSessionManager.JoinedPlayers.Count >= maxPlayers)
        {
            OnGameStarted();
        }

        Debug.Log($"Player {player.PlayerID} Joined");
    }
    private void OnPlayerLeft(PlayerRef player)
    {
        if (!HasStateAuthority) return;
        if (!_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject)) return;
        Object.Runner.Despawn(networkObject);
        _spawnedCharacters.Remove(player);
    }

    private void OnGameStarted()
    {
        Debug.Log($"Game Started");
        foreach (var playerSpawn in _networkSessionManager.JoinedPlayers)
        {
            var networkObject = Object.Runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, playerSpawn);
            _spawnedCharacters.Add(playerSpawn, networkObject);
        }
    }

}
