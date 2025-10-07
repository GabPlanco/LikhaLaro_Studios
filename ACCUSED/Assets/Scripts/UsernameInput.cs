using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UsernameInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputUsername;
    [SerializeField] private Button saveUsername;

    private const string PlayerNameKey = "PlayerName";

    private void Start()
    {
        // Load previously saved name
        if (PlayerPrefs.HasKey(PlayerNameKey))
        {
            string savedName = PlayerPrefs.GetString(PlayerNameKey);
            inputUsername.text = savedName;
        }

        saveUsername.onClick.AddListener(SaveName);
    }

    // Called when user presses "Confirm" or "Continue"
    public void SaveName()
    {
        string enteredName = inputUsername.text.Trim();

        if (!string.IsNullOrEmpty(enteredName))
        {
            PlayerPrefs.SetString(PlayerNameKey, enteredName);
            PlayerPrefs.Save(); // optional but good practice
            Debug.Log("Saved name: " + enteredName);
        }
    }

    public static string GetSavedName()
    {
        return PlayerPrefs.GetString(PlayerNameKey, "Player");
    }
}
