using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    public static string LocalSavedName = ""; // set from input UI before joining

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>(
        "",
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Send our saved name to the server once spawned
            SubmitNameServerRpc(LocalSavedName);
        }
    }

    [ServerRpc]
    private void SubmitNameServerRpc(string newName)
    {
        PlayerName.Value = newName;
    }
}
