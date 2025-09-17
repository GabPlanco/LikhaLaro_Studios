using Unity.Netcode;
using UnityEngine;

public class GameSceneSpawner : NetworkBehaviour
{
    public GameObject playerPrefab;

    private void Start()
    {
        if (IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsIds)
            {
                var spawnPos = GetSpawnPoint();
                var player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client);
            }
        }
    }

    private Vector3 GetSpawnPoint()
    {
        return new Vector3(Random.Range(-5f, 5f), 1, Random.Range(-5f, 5f));
    }
}