using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TaskProgressBar : MonoBehaviour
{
    public static TaskProgressBar Instance;

    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject winnerScreen;
    [SerializeField] private TMP_Text winnerText;

    private void Awake()
    {
        Instance = this;
        winnerScreen.SetActive(false);
    }

    public void UpdateBar(int current, int max)
    {
        progressBar.maxValue = max;
        progressBar.value = current;
    }

    public void ShowWinScreen(bool innocentsWin)
    {
        winnerScreen.SetActive(true);
        winnerText.text = innocentsWin ? "INNOCENTS WIN!" : "ASSASSIN WINS!";
    }

    /* public void ShowAssassinWinScreen(bool assassinsWin)
    {
        winnerScreen.SetActive(true);
        winnerText.text = assassinsWin ? "ASSASSIN WINS!" : "INNOCENTS WIN!";
    } */
}
