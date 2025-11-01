using Unity.Netcode;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameSceneManager : NetworkBehaviour
{
    public static GameSceneManager Instance;

    [SerializeField] GameObject meetingPanel;
    // [SerializeField] GameObject emergencyMeetingDisplay;
    // [SerializeField] GameObject bodyReportedDisplay;

    public GameObject playerPrefab;

    private readonly List<DeadBody> spawnedBodies = new List<DeadBody>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Listen for when the GameScene finishes loading for everyone
        NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoadComplete;
    }

    private void OnSceneLoadComplete(string sceneName, LoadSceneMode mode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != "GameScene") return;

        Debug.Log("[Server] All clients finished loading GameScene — spawning players.");
        SpawnPlayers();
    }

    private new void OnDestroy()
    {
        if (NetworkManager != null && NetworkManager.SceneManager != null)
            NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoadComplete;
    }


    private void Awake()
    {
        Instance = this;
    }

    private void SpawnPlayers()
    {
        var allClients = NetworkManager.Singleton.ConnectedClientsIds.ToList();
        int assassinIndex = Random.Range(0, allClients.Count);

        for (int i = 0; i < allClients.Count; i++)
        {
            var clientId = allClients[i];
            var spawnPos = GetSpawnPoint();
            var player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            var playerObj = player.GetComponent<NetworkObject>();

            playerObj.SpawnAsPlayerObject(clientId);

            // Assign role
            var roleComp = player.GetComponent<PlayerRoleComponent>();
            if (roleComp != null)
            {
                var role = (i == assassinIndex) ? PlayerRole.Assassin : PlayerRole.Innocent;
                roleComp.SetRoleServerRpc(role);
            }
        }
    }

    private Vector3 GetSpawnPoint()
    {
        return new Vector3(Random.Range(-5f, 5f), 1, Random.Range(-5f, 5f));
    }

    public void RegisterDeadBody(DeadBody body)
    {
        if (!spawnedBodies.Contains(body))
            spawnedBodies.Add(body);
    }

    [ClientRpc]
    public void StartEmergencyMeetingClientRpc()
    {
        // Clean up all dead bodies across the network
        if (IsServer)
        {
            foreach (var body in spawnedBodies)
            {
                if (body != null)
                    body.CleanupBody();
            }
            spawnedBodies.Clear();
        }

        meetingPanel?.SetActive(true);
        var panel = meetingPanel.GetComponent<MeetingPanel>();
        panel?.BeginMeeting(isEmergency: true);
        // emergencyMeetingDisplay?.SetActive(true);
        // bodyReportedDisplay?.SetActive(false);

        // ForceRefreshMeetingPanelClientRpc();
    }

    [ClientRpc]
    public void StartBodyReportClientRpc()
    {
        // Clean up all dead bodies across the network
        if (IsServer)
        {
            foreach (var body in spawnedBodies)
            {
                if (body != null)
                    body.CleanupBody();
            }
            spawnedBodies.Clear();
        }

        meetingPanel.SetActive(true);
        var panel = meetingPanel.GetComponent<MeetingPanel>();
        panel?.BeginMeeting(isEmergency: false);
        // emergencyMeetingDisplay?.SetActive(false);
        // bodyReportedDisplay?.SetActive(true);
    }

    /* [ClientRpc]
    private void ForceRefreshMeetingPanelClientRpc()
    {
        if (meetingPanel != null && meetingPanel.activeInHierarchy)
        {
            var mp = meetingPanel.GetComponent<MeetingPanel>();
            mp?.RefreshPlayerList();
        }
    } */
}

public struct PlayerInfoData
{
    public ulong ClientId;
    public string Name;
    public bool IsDead;
}

public static class PlayerUtils
{
    public static List<PlayerInfoData> GetAllPlayerInfo()
    {
        return PlayerNameDisplay.AllPlayers
            .Where(p => p != null)
            .Select(p =>
            {
                var state = p.GetComponent<PlayerState>();
                bool isDead = state != null && state.IsDead.Value;
                string name = p.playerName.Value.ToString();
                // return string.IsNullOrEmpty(name) ? $"Player {p.OwnerClientId}" : name;
                if (string.IsNullOrEmpty(name)) name = $"Player {p.OwnerClientId}";
                return new PlayerInfoData
                {
                    ClientId = p.OwnerClientId,
                    Name = name,
                    IsDead = isDead
                };
            })
            .ToList();
    }

    public static string GetPlayerNameByClientId(ulong clientId)
    {
        var player = PlayerNameDisplay.AllPlayers.FirstOrDefault(p => p != null && p.OwnerClientId == clientId);
        if (player != null && !string.IsNullOrEmpty(player.playerName.Value.ToString()))
        {
            return player.playerName.Value.ToString();
        }
        return $"Player {clientId}";
    }
}
