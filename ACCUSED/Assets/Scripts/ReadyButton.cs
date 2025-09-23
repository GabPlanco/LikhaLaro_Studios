using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Threading.Tasks;

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
        readyBtn.gameObject.SetActive(playerCount >= 2); // show only if at least 2 players
    }

    public async void OnReadyClicked()
    {
        // Host triggers loading scene first
        // SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);

        if (NetworkManager.Singleton.IsHost)
        {
            // Despawn existing players
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player != null && player.IsSpawned)
                {
                    player.Despawn();
                }
            }

            await Task.Yield();

            // Host triggers scene change for everyone
            NetworkManager.Singleton.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }
    }
}
