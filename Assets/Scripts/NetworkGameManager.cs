using UnityEngine;
using Fusion;
using System.Collections.Generic;
public class NetworkGameManager : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef playerPrefab;

    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();
    
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
    public override void Despawned()
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
            //start game count down then spawn players
        }

        Debug.Log($)
    }
    private void OnPlayerLeft(PlayerRef player)
    {

    }

}
