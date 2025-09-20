using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PublicNetworkButtons : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    // [SerializeField] private Button serverBtn;
    [SerializeField] private Button clientBtn;

    private void Awake()
    {
        hostBtn.onClick.AddListener(() => {
            TestLobby.CreateLobbyAndRelay(createdCode =>
            {
                NetworkManager.Singleton.SceneManager.LoadScene("PrivateServer", LoadSceneMode.Single); // Replace with your actual scene name
                NetworkCode.CodeText = createdCode;
            });
            // NetworkManager.Singleton.StartHost();
        });
        /* serverBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        }); */
        clientBtn.onClick.AddListener(() => {
            TestLobby.QuickJoinLobby(onJoined =>
            {
                NetworkManager.Singleton.SceneManager.LoadScene("PrivateServer", LoadSceneMode.Single); // Replace with your actual scene name
                if (onJoined) Debug.Log("Joined automatically!");
            });
            // NetworkManager.Singleton.StartClient();
        });
    }
}
