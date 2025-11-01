using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class ReadyButton : MonoBehaviour
{
    [SerializeField] private Button readyBtn;

    private void Start()
    {
        // Hide button by default
        readyBtn.gameObject.SetActive(false);

        readyBtn.onClick.AddListener(OnReadyClicked);

        // Only the host should control this
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerCountChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerCountChanged;

            // Initial check
            OnPlayerCountChanged(0);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerCountChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerCountChanged;
        }
    }

    private void OnPlayerCountChanged(ulong _)
    {
        int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
        Debug.Log("Player count: " + playerCount);
        readyBtn.gameObject.SetActive(playerCount >= 1); // show only if at least 1 players
    }

    public void OnReadyClicked()
    {
        // Host triggers loading scene first
        // SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);
        if (NetworkManager.Singleton.IsHost)
            StartCoroutine(HostTransitionToGameScene());
    }

    private IEnumerator HostTransitionToGameScene()
    {
        // Despawn existing players safely
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject;
            if (player != null && player.IsSpawned)
            {
                player.Despawn();
            }
        }

        // ?? Wait one frame to let Netcode sync the despawns
        yield return null;

        Debug.Log("[Host] Loading GameScene for all clients...");
        NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
}
