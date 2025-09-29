using Unity.Netcode;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameSceneManager : NetworkBehaviour
{
    public GameObject playerPrefab;

    private void Start()
    {
        // Ensure this only runs in GameScene
        if (SceneManager.GetActiveScene().name != "GameScene")
            return;

        if (IsServer)
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
    }

    private Vector3 GetSpawnPoint()
    {
        return new Vector3(Random.Range(-5f, 5f), 1, Random.Range(-5f, 5f));
    }
}
