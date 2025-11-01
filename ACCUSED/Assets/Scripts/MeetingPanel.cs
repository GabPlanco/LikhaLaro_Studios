using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MeetingPanel : MonoBehaviour
{
    [Header("Main Components")]
    [SerializeField] private GameObject playerListPanel;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Transform chatContent;
    [SerializeField] private TMP_Text chatMessagePrefab;
    [SerializeField] private GameObject emergencyMeetingDisplay;
    [SerializeField] private GameObject bodyReportedDisplay;

    [Header("Player Slot Prefab")]
    [SerializeField] private GameObject playerSlotPrefab;

    private readonly List<PlayerSlot> playerSlots = new List<PlayerSlot>();
    private Dictionary<ulong, PlayerSlot> slotMap = new Dictionary<ulong, PlayerSlot>();

    // For testing vote tallies
    private readonly Dictionary<string, int> voteCounts = new Dictionary<string, int>();

    public static bool meetingPanelIsActive = false;

    private void OnEnable()
    {
        // PlayerNameDisplay.OnPlayerListChanged += RefreshPlayerList;
        // PlayerState.OnAnyDeathChanged += UpdateDeadStatus;

        // StartCoroutine(RefreshAfterDelay());

        RefreshPlayerList(); // initial populate
    }

    private void OnDisable()
    {
        // PlayerNameDisplay.OnPlayerListChanged -= RefreshPlayerList;
        // PlayerState.OnAnyDeathChanged -= UpdateDeadStatus;

        StopCoroutine(TestCountdownSequence());
    }

    /* private IEnumerator RefreshAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // allow network vars to sync
        RefreshPlayerList();
    } */

    private void Start()
    {
        // UpdateCountdown(30);

        sendButton.onClick.AddListener(() =>
        {
            AddChatMessage("You", chatInputField.text);
            chatInputField.text = "";
        });

        // Start the test countdown sequence
        // StartCoroutine(TestCountdownSequence());
    }

    public void BeginMeeting(bool isEmergency)
    {
        meetingPanelIsActive = true;

        // Reset all votes and chat
        voteCounts.Clear();

        foreach (Transform child in chatContent)
            Destroy(child.gameObject);

        // Reset player UI
        RefreshPlayerList();

        // Reset countdown text
        countdownText.text = "";

        // Show proper labels or icons
        emergencyMeetingDisplay?.SetActive(isEmergency);
        bodyReportedDisplay?.SetActive(!isEmergency);

        // Start the new countdown sequence
        StartCoroutine(TestCountdownSequence());
    }

    public void RefreshPlayerList()
    {
        var names = PlayerUtils.GetAllPlayerInfo();
        CreatePlayerSlots(names.ToArray());
    }

    public void CreatePlayerSlots(PlayerInfoData[] playerNames)
    {
        foreach (Transform child in playerListPanel.transform)
            Destroy(child.gameObject);

        playerSlots.Clear();
        slotMap.Clear();

        float slotHeight = 100f;  // height of one slot
        float spacing = 10f;      // vertical gap between slots

        for (int i = 0; i < playerNames.Length; i++)
        {
            var slot = Instantiate(playerSlotPrefab, playerListPanel.transform);
            var rect = slot.GetComponent<RectTransform>();

            // Set proper scale
            rect.localScale = Vector3.one;

            // Manually position each slot downward
            rect.anchoredPosition = new Vector2(0, -(slotHeight + spacing) * i);

            // Optional: set size if prefab doesn’t define it
            rect.sizeDelta = new Vector2(600f, slotHeight);

            var ui = slot.GetComponent<PlayerSlot>();

            ui.Setup(playerNames[i], OnVoteButtonPressed);
            ui.HideVoteButton();
            playerSlots.Add(ui);
            slotMap[playerNames[i].ClientId] = ui;
        }

        UpdateDeadStatus();

        // Resize the parent panel if needed (so scroll works)
        var parentRect = playerListPanel.GetComponent<RectTransform>();
        parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x, playerNames.Length * (slotHeight + spacing));
    }

    private void UpdateDeadStatus()
    {
        Debug.Log("[MeetingPanel] --- UpdateDeadStatus() called ---");

        var allPlayers = FindObjectsOfType<PlayerState>();
        Debug.Log($"[MeetingPanel] Found {allPlayers.Length} PlayerState objects.");

        foreach (var p in allPlayers)
        {
            var nameComp = p.GetComponentInChildren<PlayerNameDisplay>();
            if (nameComp == null)
            {
                Debug.LogWarning("[MeetingPanel] Skipping player with no name component.");
                continue;
            }

            string pname = nameComp.playerName.Value.ToString();
            bool dead = p.IsDead.Value;
            Debug.Log($"[MeetingPanel] Checking {pname}  ?  IsDead={dead}");

            ulong cid = p.OwnerClientId;
            if (slotMap.ContainsKey(cid))
            {
                Debug.Log($"[MeetingPanel]   Updating slot for {pname}");
                slotMap[cid].SetDead(p.IsDead.Value);
            }
            else
            {
                Debug.LogWarning($"[MeetingPanel]   No slot found for {pname}");
            }
        }
    }

    private void OnVoteButtonPressed(string playerName)
    {
        if (!voteCounts.ContainsKey(playerName))
            voteCounts[playerName] = 0;

        voteCounts[playerName]++;
        Debug.Log($"Voted for {playerName}. Current votes: {voteCounts[playerName]}");
    }

    public void AddChatMessage(string sender, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        if (chatMessagePrefab == null)
        {
            Debug.LogError("[MeetingPanel] chatMessagePrefab is null!");
            return;
        }
        if (chatContent == null)
        {
            Debug.LogError("[MeetingPanel] chatContent has been destroyed!");
            return;
        }

        var chatLine = Instantiate(chatMessagePrefab, chatContent);
        if (chatLine == null)
        {
            Debug.LogError("[MeetingPanel] Failed to instantiate chat line!");
            return;
        }
        chatLine.text = $"<b>{sender}:</b> {message}";

        Canvas.ForceUpdateCanvases();
        var scroll = chatContent.GetComponentInParent<ScrollRect>();
        if (scroll != null)
        {
            scroll.verticalNormalizedPosition = 0;
        }
    }

    private IEnumerator TestCountdownSequence()
    {
        // Phase 1: Initial discussion (5 seconds)
        yield return StartCoroutine(CountdownTimer(5, "Discussion Phase"));

        // Show all vote buttons
        foreach (var ui in playerSlots)
            ui.ShowVoteButton();

        // Phase 2: Voting phase (10 seconds)
        Debug.Log("Voting Phase begins!");
        yield return StartCoroutine(CountdownTimer(10, "Voting Phase"));

        // End of meeting
        Debug.Log("Meeting ended. Vote tally:");
        foreach (var kvp in voteCounts)
        {
            Debug.Log($"{kvp.Key}: {kvp.Value} votes");
        }

        // Close the meeting panel
        meetingPanelIsActive = false;
        gameObject.SetActive(false);
    }

    private IEnumerator CountdownTimer(int seconds, string label)
    {
        for (int i = seconds; i >= 0; i--)
        {
            countdownText.text = $"{label}: {i}";
            yield return new WaitForSeconds(1f);
        }
    }
}
