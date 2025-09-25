using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

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
        CurrentProgress.OnValueChanged += OnProgressChanged;

        // Subscribe to death events for all players
        foreach (var state in FindObjectsOfType<PlayerState>())
        {
            state.IsDead.OnValueChanged += OnAnyPlayerDeath;
        }
    }

    private new void OnDestroy()
    {
        CurrentProgress.OnValueChanged -= OnProgressChanged;

        foreach (var state in FindObjectsOfType<PlayerState>())
        {
            state.IsDead.OnValueChanged -= OnAnyPlayerDeath;
        }
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

    // === Assassin win condition ===
    private void OnAnyPlayerDeath(bool oldValue, bool newValue)
    {
        if (!IsServer) return;

        if (newValue == true) // a player just died
        {
            CheckAssassinWin();
        }
    }

    private void CheckAssassinWin()
    {
        // Get all players in the scene
        var players = FindObjectsOfType<PlayerRoleComponent>();

        bool anyInnocentAlive = players.Any(p =>
        {
            var state = p.GetComponent<PlayerState>();
            return p.Role == PlayerRole.Innocent && state != null && !state.IsDead.Value;
        });

        if (!anyInnocentAlive)
        {
            AnnounceAssassinWinClientRpc();
        }
    }

    [ClientRpc]
    private void AnnounceAssassinWinClientRpc()
    {
        TaskProgressBar.Instance?.ShowWinScreen(false); // Assassin wins
    }
}
