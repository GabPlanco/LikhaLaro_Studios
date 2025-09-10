using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ReadyButton : MonoBehaviour
{
    [SerializeField] private Button readyBtn;

    private void Start()
    {
        readyBtn.gameObject.SetActive(false);

        readyBtn.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsHost)
            {
                // Host moves everyone to the new scene
                NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
            }
        });
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;

            // Show Ready button only when at least 4 players are in
            readyBtn.gameObject.SetActive(playerCount >= 4);
        }
        else
        {
            // Clients never see the Ready button
            readyBtn.gameObject.SetActive(false);
        }
    }
}
