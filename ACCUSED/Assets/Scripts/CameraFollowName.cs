using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CameraFollowName : NetworkBehaviour
{
    [SerializeField] private Transform target;     // The player head or body to follow
    [SerializeField] private Vector3 offset = new Vector3(0, 0f, 0); // Height offset above head
    [SerializeField] private Canvas topPlayerDisplay;
    private Camera mainCamera;
    private PlayerState playerState;

    private void Start()
    {
        // Automatically find the main camera
        mainCamera = Camera.main;

        // Find parent player (who owns this name tag)
        playerState = GetComponentInParent<PlayerState>();

        // If not manually assigned, try to find the player root automatically
        if (target == null)
        {
            // This assumes the script is attached to the name tag inside the player prefab
            target = transform.root;
        }

        if (playerState != null)
        {
            // Subscribe to death state
            playerState.IsDead.OnValueChanged += OnDeathStateChanged;

            // Initial visibility
            ApplyVisibilityForLocalViewer(playerState.IsDead.Value);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Follow player's position + offset
        transform.position = target.position + offset;

        // Always face the main camera
        if (mainCamera != null)
        {
            transform.LookAt(mainCamera.transform);
            transform.Rotate(0, 180f, 0); // Fix rotation to face camera properly
        }
    }

    private void OnDeathStateChanged(bool oldValue, bool newValue)
    {
        ApplyVisibilityForLocalViewer(newValue);
    }

    private void ApplyVisibilityForLocalViewer(bool targetIsDead)
    {
        var localViewer = GetLocalPlayerState();
        bool viewerIsDead = localViewer != null && localViewer.IsDead.Value;

        if (!viewerIsDead)
        {
            if (targetIsDead)
            {
                // Alive seeing dead ? invisible
                SetNameTagVisible(false);
            }
            else
            {
                // Dead seeing dead ? faint tag
                SetNameTagVisible(true);
            }
        }
        else
        {
            // Target alive ? always visible
            SetNameTagVisible(true);
        }
    }

    private void SetNameTagVisible(bool visible)
    {
        if (topPlayerDisplay != null)
            topPlayerDisplay.enabled = visible;
    }

    private PlayerState GetLocalPlayerState()
    {
        if (NetworkManager.Singleton == null)
            return null;

        var localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayer != null)
            return localPlayer.GetComponent<PlayerState>();

        return null;
    }

    private new void OnDestroy()
    {
        if (playerState != null)
            playerState.IsDead.OnValueChanged -= OnDeathStateChanged;
    }
}
