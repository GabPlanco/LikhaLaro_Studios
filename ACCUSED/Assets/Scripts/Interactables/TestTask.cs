using Unity.Netcode;
using UnityEngine;

public class TestTask : Interactables
{
    [SerializeField] private int progressValue = 1; // how much this task adds

    protected override void Interact()
    {
        // var role = GetComponent<PlayerRoleComponent>();
        // if (role == null || role.Role != PlayerRole.Innocent) return;

        // Tell server we completed this task
        GameProgressManager.Instance?.AddProgressServerRpc(progressValue);
    }
}
