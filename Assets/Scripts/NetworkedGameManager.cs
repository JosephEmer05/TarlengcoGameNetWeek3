using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Fusion;
using TMPro;
using UnityEngine;

namespace Network
{
    public class NetworkedGameManager : NetworkBehaviour
    {
        [SerializeField] private NetworkPrefabRef playerPrefab;
        [SerializeField] private TextMeshProUGUI _playerCountText;
        [SerializeField] private TextMeshProUGUI _timerCountText;
        [SerializeField] private Transform[] team1Spawns;
        [SerializeField] private Transform[] team2Spawns;

        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();

        [Header("Settings")]
        [SerializeField] private int maxPlayers = 2;
        [SerializeField] private int timerBeforeStart = 3;

        #region Networked Properties
        [Networked] public int NetworkedPlayerCount { get; set; }
        [Networked] public TickTimer RoundStartTimer { get; set; }
        [Networked] public NetworkBool GameHasStarted { get; set; }
        #endregion

        public override void Spawned()
        {
            NetworkSessionManager.Instance.OnPlayerJoinedEvent += OnPlayerJoined;
            NetworkSessionManager.Instance.OnPlayerLeftEvent += OnPlayerLeft;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (NetworkSessionManager.Instance != null)
            {
                NetworkSessionManager.Instance.OnPlayerJoinedEvent -= OnPlayerJoined;
                NetworkSessionManager.Instance.OnPlayerLeftEvent -= OnPlayerLeft;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (HasStateAuthority)
            {
                NetworkedPlayerCount = Runner.ActivePlayers.Count();

                if (!GameHasStarted && RoundStartTimer.Expired(Runner) && RoundStartTimer.IsRunning)
                {
                    GameHasStarted = true;
                    OnGameStarted();
                }
            }
        }

        public override void Render()
        {
            if (_playerCountText != null)
                _playerCountText.text = $"Players: {NetworkedPlayerCount}/{maxPlayers}";

            if (RoundStartTimer.IsRunning)
            {
                float? remaining = RoundStartTimer.RemainingTime(Runner);
                _timerCountText.text = remaining.HasValue ? Mathf.CeilToInt(remaining.Value).ToString() : "";
            }
            else
            {
                _timerCountText.text = "";
            }
        }

        private void OnPlayerJoined(PlayerRef player)
        {
            if (!HasStateAuthority) return;

            if (Runner.ActivePlayers.Count() >= maxPlayers && !GameHasStarted)
            {
                RoundStartTimer = TickTimer.CreateFromSeconds(Runner, timerBeforeStart);
            }
        }

        private void OnPlayerLeft(PlayerRef player)
        {
            if (!HasStateAuthority) return;
            if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
            {
                Runner.Despawn(networkObject);
                _spawnedCharacters.Remove(player);
            }
        }

        private void OnGameStarted()
        {
            foreach (var playerRef in Runner.ActivePlayers)
            {
                if (playerRef == Runner.LocalPlayer && Runner.IsServer) continue;

                NetworkObject NO = Runner.Spawn(playerPrefab, Vector3.zero, Quaternion.identity, playerRef);
                _spawnedCharacters.Add(playerRef, NO);
                StartCoroutine(PositionPlayer(NO));
            }
        }

        private IEnumerator PositionPlayer(NetworkObject obj)
        {
            yield return new WaitForSeconds(0.2f);
            NetworkPlayer script = obj.GetComponent<NetworkPlayer>();

            Transform spawnPoint = script.TeamID == 1
                ? team1Spawns[Random.Range(0, team1Spawns.Length)]
                : team2Spawns[Random.Range(0, team2Spawns.Length)];

            obj.transform.position = spawnPoint.position;
            script.NetworkedPosition = spawnPoint.position;
        }
    }
}