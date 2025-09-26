using Unity.Netcode;
using UnityEngine;

public class TestTask : Interactables
{
    [SerializeField] private int progressValue = 1; // how much this task adds

    protected override void Interact()
    {
        var interactor = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (interactor == null) return;

        var roleComp = interactor.GetComponent<PlayerRoleComponent>();
        if (roleComp == null || roleComp.Role != PlayerRole.Innocent) return;

        // Tell server we completed this task
        GameProgressManager.Instance?.AddProgressServerRpc(progressValue);
    }
}
