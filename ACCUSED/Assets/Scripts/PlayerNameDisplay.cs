using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Collections;

public class PlayerNameDisplay : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private TextMeshProUGUI playerNameDisplay;

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnNameChanged;

        // Apply the current name immediately
        OnNameChanged("", playerName.Value);

        if (IsOwner && IsClient)
        {
            // Send our local name to the server
            string localName = UsernameInput.GetSavedName();
            SetNameServerRpc(localName);
        }
    }

    [ServerRpc]
    private void SetNameServerRpc(string newName)
    {
        playerName.Value = newName;
    }

    private void OnNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        if (playerNameDisplay != null)
        {
            playerNameDisplay.text = newName.ToString();
        }
    }

    private new void OnDestroy()
    {
        playerName.OnValueChanged -= OnNameChanged;
    }
}
