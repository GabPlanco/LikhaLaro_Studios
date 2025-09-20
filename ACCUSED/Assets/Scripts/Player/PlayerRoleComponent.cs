using Unity.Netcode;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerRoleComponent : NetworkBehaviour
{
    private NetworkVariable<PlayerRole> role = new NetworkVariable<PlayerRole>(
        PlayerRole.Innocent,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    [SerializeField] private GameObject innocentDisplay;
    [SerializeField] private GameObject assassinDisplay;

    public PlayerRole Role => role.Value;

    public override void OnNetworkSpawn()
    {
        // Only react when in GameScene
        if (SceneManager.GetActiveScene().name != "GameScene")
            return;

        if (IsOwner)
        {
            role.OnValueChanged += OnRoleChanged;

            // Apply immediately if role already set
            OnRoleChanged(PlayerRole.Innocent, role.Value);
        }
    }

    private void OnRoleChanged(PlayerRole oldValue, PlayerRole newValue)
    {
        if (!IsOwner) return; // only show local player's role

        innocentDisplay.SetActive(newValue == PlayerRole.Innocent);
        assassinDisplay.SetActive(newValue == PlayerRole.Assassin);

        // Hide again after 3 seconds
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        innocentDisplay.SetActive(false);
        assassinDisplay.SetActive(false);
    }

    // Called by GameSceneManager on spawn
    [ServerRpc(RequireOwnership = false)]
    public void SetRoleServerRpc(PlayerRole newRole)
    {
        if (IsServer)
        {
            role.Value = newRole;
        }
    }
}
