using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TestEmergency : Interactables
{
    protected override void Interact()
    {
        var interactor = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (interactor == null) return;

        RequestStartEmergencyMeetingServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestStartEmergencyMeetingServerRpc(ServerRpcParams rpcParams = default)
    {
        // The server receives the request and then triggers it for everyone
        GameSceneManager.Instance?.StartEmergencyMeetingClientRpc();
    }
}
