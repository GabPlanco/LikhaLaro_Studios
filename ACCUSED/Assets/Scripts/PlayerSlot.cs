using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Button voteButton;
    [SerializeField] private GameObject deadPlayerImage;

    // private string playerName;
    private PlayerInfoData playerInfoData;
    private System.Action<string> onVote;

    public void Setup(PlayerInfoData info, System.Action<string> onVoteCallback)
    {
        playerInfoData = info;
        playerNameText.text = info.Name;
        onVote = onVoteCallback;

        voteButton.onClick.AddListener(() => onVote?.Invoke(playerInfoData.Name));
        SetDead(info.IsDead);
    }

    public void SetDead(bool isDead)
    {
        Debug.Log($"[PlayerSlot] Setting {playerInfoData.Name} dead={isDead}");
        deadPlayerImage.SetActive(isDead);

        // Disable voting if dead
        if (voteButton != null)
        {
            voteButton.interactable = !isDead;
        }
    }

    public void HideVoteButton()
    {
        if (voteButton != null)
            voteButton.gameObject.SetActive(false);
    }
    public void ShowVoteButton()
    {
        if (voteButton != null)
            voteButton.gameObject.SetActive(true);
    }
}
