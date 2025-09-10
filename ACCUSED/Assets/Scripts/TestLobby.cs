using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;

public class TestLobby : MonoBehaviour
{
    // create Relay + Lobby
    public static async void CreateLobbyAndRelay(System.Action<string> onCreated)
    {
        try
        {
            // 1. Create Relay allocation (same as before)
            Allocation allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(9);
            string joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // 2. Create Lobby that stores joinCode
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(
                "MyLobby", // lobby name
                10,        // max players
                new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>
                    {
                        { "joinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
                    }
                }
            );

            Debug.Log("Lobby created with joinCode: " + joinCode);

            // 3. Start host normally
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            onCreated?.Invoke(joinCode);
        }
        catch (LobbyServiceException e) { Debug.LogError(e); }
    }

    // find a lobby and join automatically
    public static async void QuickJoinLobby(System.Action<bool> onJoined)
    {
        try
        {
            // 1. Query all public lobbies
            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync();

            if (response.Results.Count == 0)
            {
                Debug.Log("No lobbies found!");
                onJoined?.Invoke(false);
                return;
            }

            // 2. Get joinCode from first available lobby
            Lobby lobby = response.Results[0];
            string joinCode = lobby.Data["joinCode"].Value;

            Debug.Log("Auto-joining with code: " + joinCode);

            // 3. Call your existing Relay join
            TestRelay.JoinRelay(joinCode, onJoined);
        }
        catch (LobbyServiceException e) { Debug.LogError(e); }
    }
}
