using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class DeadBody : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [HideInInspector] public new ulong OwnerClientId;
    // [HideInInspector] public string PlayerName;

    private bool isMarkedForCleanup = false;

    public void Setup(ulong ownerId, string name)
    {
        OwnerClientId = ownerId;
        PlayerName.Value = name;
    }
    private void OnEnable()
    {
        Debug.Log($"[DeadBody] Spawned for {PlayerName.Value} (Owner: {OwnerClientId})");
    }

    private new void OnDestroy()
    {
        Debug.Log($"[DeadBody] Destroyed for {PlayerName.Value}");
    }


    private void Start()
    {
        // Register this body to the manager if it exists
        if (GameSceneManager.Instance != null)
            GameSceneManager.Instance.RegisterDeadBody(this);
    }

    // Called by GameSceneManager when a meeting starts
    public void CleanupBody()
    {
        if (isMarkedForCleanup) return;
        isMarkedForCleanup = true;

        if (IsServer)
        {
            NetworkObject.Despawn();
            Destroy(gameObject);
        }
    }
}
