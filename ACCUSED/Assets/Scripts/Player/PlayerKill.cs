using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerKill : Interactables
{
    private PlayerRoleComponent roleComponent;

    private void Awake()
    {
        roleComponent = GetComponent<PlayerRoleComponent>();
    }

    // ----- WON'T WORK WHEN NOT IN GAMESCENE -----

    /* private void Awake()
    {
        enabled = false;
    } */

    private void OnEnable()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoaded;
    }
    private void Start()
    {
        enabled = (SceneManager.GetActiveScene().name == "GameScene");
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        // Enable only in GameScene
        enabled = (sceneName == "GameScene");
    }



    // ----- WHEN IN GAMESCENE -----

    protected override void Interact()
    {
        // if (!NetworkManager.Singleton) return;

        if (!enabled) return;

        // Only assassins can kill
        if (roleComponent == null || roleComponent.Role != PlayerRole.Assassin)
        {
            return;
        }

        // Get the target's NetworkObjectId
        ulong targetId = GetComponent<NetworkObject>().NetworkObjectId;

        // Ask server to kill this player
        RequestKillServerRpc(targetId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestKillServerRpc(ulong targetId, ServerRpcParams rpcParams = default)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var targetObj))
        {
            // Prevent killing self accidentally
            if (targetObj.OwnerClientId != rpcParams.Receive.SenderClientId)
            {
                Debug.Log($"Killing player {targetId}");
                targetObj.Despawn();
            }
        }
    }
}
