using System.Collections.Generic;
using System.Linq;
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

        [Header("Spawn Points")]
        [SerializeField] private Transform[] team1Spawns;
        [SerializeField] private Transform[] team2Spawns;

        private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new();
        private const int maxPlayers = 2;
        private const int timerBeforeStart = 3;
        private bool hasGameStarted = false;

        [Networked] public TickTimer RoundStartTimer { get; set; }

        public override void Spawned()
        {
            NetworkSessionManager.Instance.OnPlayerJoinedEvent += OnPlayerJoined;
            NetworkSessionManager.Instance.OnPlayerLeftEvent += OnPlayerLeft;
        }

        public override void FixedUpdateNetwork()
        {
            if (RoundStartTimer.Expired(Runner))
            {
                RoundStartTimer = default;
                OnGameStarted();
            }
        }

        public override void Render()
        {
            if (_playerCountText != null)
                _playerCountText.text = $"Players: {Runner.ActivePlayers.Count()}/{maxPlayers}";

            if (RoundStartTimer.IsRunning)
                _timerCountText.text = Mathf.CeilToInt(RoundStartTimer.RemainingTime(Runner) ?? 0).ToString();
            else
                _timerCountText.text = "";
        }

        private void OnPlayerJoined(PlayerRef player)
        {
            if (!HasStateAuthority) return;
            if (NetworkSessionManager.Instance.JoinedPlayers.Count >= maxPlayers)
                RoundStartTimer = TickTimer.CreateFromSeconds(Runner, timerBeforeStart);
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
            if (hasGameStarted) return;
            hasGameStarted = true;

            int i = 0;
            foreach (var playerRef in NetworkSessionManager.Instance.JoinedPlayers)
            {
                int assignedTeam = (i % 2 == 0) ? 1 : 2;
                Transform[] selectedSpawnList = (assignedTeam == 1) ? team1Spawns : team2Spawns;

                Transform selectedSpawn = selectedSpawnList[Random.Range(0, selectedSpawnList.Length)];

                var networkObject = Runner.Spawn(playerPrefab, selectedSpawn.position, selectedSpawn.rotation, playerRef);

                var playerScript = networkObject.GetComponent<NetworkPlayer>();
                if (playerScript != null)
                {
                    playerScript.NetworkedPosition = selectedSpawn.position;
                    playerScript.NetworkedRotation = selectedSpawn.rotation;
                    playerScript.TeamID = assignedTeam;
                }

                _spawnedCharacters.Add(playerRef, networkObject);
                i++;
            }
        }
    }
}