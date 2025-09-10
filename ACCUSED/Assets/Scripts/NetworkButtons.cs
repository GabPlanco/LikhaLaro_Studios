using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkButtons : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    // [SerializeField] private Button serverBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private TMP_InputField code;

    private void Awake() {
        hostBtn.onClick.AddListener(() => {
            SceneManager.LoadScene("PrivateServer"); // Replace with your actual scene name
            TestRelay.CreateRelay(createdCode =>
            {
                // ? Update UI once relay is created
                NetworkCode.CodeText = createdCode;
            });
            // NetworkManager.Singleton.StartHost();
        });
        /* serverBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        }); */
        clientBtn.onClick.AddListener(() => {
            string joinCode = code.text;
            SceneManager.LoadScene("PrivateServer"); // Replace with your actual scene name
            

            TestRelay.JoinRelay(joinCode, onJoined =>
            {
                if (onJoined == true)
                {
                    NetworkCode.CodeText = joinCode;
                }
            });
            // NetworkManager.Singleton.StartClient();
        });
    }
}
