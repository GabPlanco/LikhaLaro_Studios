using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameProgressManager : NetworkBehaviour
{
    public static GameProgressManager Instance;

    [SerializeField] private int maxProgress = 5;
    public NetworkVariable<int> CurrentProgress = new NetworkVariable<int>(0);

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        PlayerState.OnAnyPlayerDied += OnAnyPlayerDeath;
        CurrentProgress.OnValueChanged += OnProgressChanged;

        Debug.Log("[GPM] Subscribed to global OnAnyPlayerDied event.");
    }

    private new void OnDestroy()
    {
        if (IsServer)
            PlayerState.OnAnyPlayerDied -= OnAnyPlayerDeath;
        // If you subscribed via the global event above, it's good to unsubscribe — but that requires
        // storing the delegate. For brevity, assume the game lifecycle ends at destroy.
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddProgressServerRpc(int amount)
    {
        if (CurrentProgress.Value < maxProgress)
        {
            CurrentProgress.Value += amount;
            Debug.Log($"Progress updated: {CurrentProgress.Value}/{maxProgress}");

            if (CurrentProgress.Value >= maxProgress)
            {
                AnnounceInnocentWinClientRpc();
            }
        }
    }

    private void OnProgressChanged(int oldValue, int newValue)
    {
        TaskProgressBar.Instance?.UpdateBar(newValue, maxProgress);
    }

    [ClientRpc]
    private void AnnounceInnocentWinClientRpc()
    {
        TaskProgressBar.Instance?.ShowWinScreen(true);
    }

    private void OnAnyPlayerDeath(PlayerState deadPlayer)
    {
        Debug.Log($"[GPM] Received death event from {deadPlayer.OwnerClientId}");
        CheckAssassinWin();
    }

    // === Assassin win condition ===
    // Called only on server
    private void CheckAssassinWin()
    {
        if (!IsServer)
        {
            Debug.Log("[GPM] CheckAssassinWin called on non-server — ignoring.");
            return;
        }

        Debug.Log("[GPM] Running CheckAssassinWin()");

        // Option A: authoritative approach using spawned player objects
        var roleComponents = FindObjectsOfType<PlayerRoleComponent>();
        Debug.Log($"[GPM] Found {roleComponents.Length} PlayerRoleComponent(s) in scene");

        // Count alive innocents and alive assassins
        int aliveInnocents = 0;
        int aliveAssassins = 0;

        foreach (var p in roleComponents)
        {
            var state = p.GetComponent<PlayerState>();
            bool isDead = (state != null) ? state.IsDead.Value : false;
            Debug.Log($"[GPM] Player OwnerClientId={p.OwnerClientId}, Role={p.Role}, IsDead={isDead}");

            if (!isDead)
            {
                if (p.Role == PlayerRole.Innocent) aliveInnocents++;
                else if (p.Role == PlayerRole.Assassin) aliveAssassins++;
            }
        }

        Debug.Log($"[GPM] Alive counts => Innocents: {aliveInnocents}, Assassins: {aliveAssassins}");

        // assassin wins if no innocents are alive
        if (aliveInnocents == 0 && aliveAssassins > 0)
        {
            Debug.Log("[GPM] Assassin win condition met (no alive innocents). Announcing assassin win.");
            AnnounceAssassinWinClientRpc();
            return;
        }

        // Optional: edge case — if no players at all
        if (roleComponents.Length == 0)
        {
            Debug.LogWarning("[GPM] No PlayerRoleComponents found during CheckAssassinWin — possible timing issue.");
        }
    }

    [ClientRpc]
    private void AnnounceAssassinWinClientRpc()
    {
        TaskProgressBar.Instance?.ShowWinScreen(false); // Assassin wins
    }
}
