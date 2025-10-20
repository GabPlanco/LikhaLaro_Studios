using Unity.Netcode;
using UnityEngine;

public class DeadBodyInteractable : Interactables
{
    protected override void Interact()
    {
        // Only local player triggers the meeting UI
        // if (!IsOwner && !IsClient) return;

        // var interactor = NetworkManager.Singleton.LocalClient?.PlayerObject;
        // if (interactor == null) return;

        // Notify server that a body was reported
        BodyReportServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void BodyReportServerRpc(ServerRpcParams rpcParams = default)
    {
        // You can add logic here to mark who reported, or disable this body
        Debug.Log("Body reported! Starting meeting...");

        // Sync to all clients
        GameSceneManager.Instance?.StartBodyReportClientRpc();
    }
}
