using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Collections;

public class PlayerNameDisplay : NetworkBehaviour
{
    public static readonly List<PlayerNameDisplay> AllPlayers = new List<PlayerNameDisplay>();

    public static event Action OnPlayerListChanged;
    public static event Action OnPlayerNameChanged;

    public NetworkVariable<FixedString32Bytes> playerName = new NetworkVariable<FixedString32Bytes>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private TextMeshProUGUI playerNameDisplay;

    public override void OnNetworkSpawn()
    {
        AllPlayers.Add(this);
        playerName.OnValueChanged += HandleNameChanged;

        OnPlayerListChanged?.Invoke();

        // Apply immediately
        HandleNameChanged("", playerName.Value);

        if (IsOwner && IsClient)
        {
            string localName = UsernameInput.GetSavedName();
            SetNameServerRpc(localName);
        }
    }

    public override void OnNetworkDespawn()
    {
        AllPlayers.Remove(this);
        playerName.OnValueChanged -= HandleNameChanged;

        OnPlayerListChanged?.Invoke();
    }

    [ServerRpc]
    private void SetNameServerRpc(string newName)
    {
        playerName.Value = newName;
    }

    private void HandleNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        if (playerNameDisplay != null)
        {
            playerNameDisplay.text = newName.ToString();
        }

        OnPlayerNameChanged?.Invoke();
    }
}
