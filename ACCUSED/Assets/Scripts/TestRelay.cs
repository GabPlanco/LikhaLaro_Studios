using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TestRelay : MonoBehaviour
{
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public static async void CreateRelay(Action<string> onCreated)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(9);

            string createdCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(createdCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            /* older method
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            */

            NetworkManager.Singleton.StartHost();

            // ? Send back the code to whoever called this method
            onCreated?.Invoke(createdCode);

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public static async void JoinRelay(string joinCode, Action<bool> onJoined)
    {
        try
        {
            Debug.Log("Using " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            /* older method
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
            */

            // Subscribe BEFORE starting client
            NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
            {
                if (id == NetworkManager.Singleton.LocalClientId)
                {
                    Debug.Log("Successfully joined relay!");
                    onJoined?.Invoke(true);
                }
            };

            NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
            {
                if (id == NetworkManager.Singleton.LocalClientId)
                {
                    Debug.LogWarning("Failed to join relay or got disconnected!");
                    onJoined?.Invoke(false);
                }
            };

            NetworkManager.Singleton.StartClient();

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
