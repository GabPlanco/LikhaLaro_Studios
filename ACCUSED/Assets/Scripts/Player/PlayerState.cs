using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerState : NetworkBehaviour
{
    [SerializeField] private Material aliveMaterial;
    [SerializeField] private Material deadMaterial;
    [SerializeField] private Material ghostMaterial;

    // Server-owned state
    public NetworkVariable<bool> IsDead = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Renderer[] renderers;

    private void Awake()
    {
        // collect all renderers (including children)
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public override void OnNetworkSpawn()
    {
        // Always subscribe so we get updates when someone's death state changes
        IsDead.OnValueChanged += OnIsDeadChanged;

        // Apply initial visibility (important for clients that join mid-game)
        ApplyVisibilityForLocalViewer(IsDead.Value);
    }

    public override void OnNetworkDespawn()
    {
        IsDead.OnValueChanged -= OnIsDeadChanged;
    }

    private void OnIsDeadChanged(bool previousValue, bool newValue)
    {
        // Called on every client when this player's IsDead changes.
        // Update appearance for the local viewer.
        ApplyVisibilityForLocalViewer(newValue);

        // If this is the *local viewer* that just died,
        // reapply visibility for all other players too
        if (IsOwner && newValue == true)
        {
            ReapplyVisibilityForAllPlayers();
        }
    }

    // Decide what the local viewer should see for THIS target player.
    // If local viewer is alive and target is dead => hide target entirely.
    // If local viewer is dead and target is dead  => show target but faded.
    // Otherwise (target alive) => show normally.
    private void ApplyVisibilityForLocalViewer(bool targetIsDead)
    {
        var localViewerState = GetLocalPlayerState();

        bool viewerIsDead = localViewerState != null && localViewerState.IsDead.Value;

        /* // If target is local player, you may want to always show (owner's own view),
        // or handle differently. Below, owner always sees themselves normally.
        if (IsOwner)
        {
            // local player sees themselves normally (you can tweak this if you want)
            SetMaterial(aliveMaterial);
            return;
        } */

        if (!viewerIsDead)
        {
            if (targetIsDead)
            {
                // Viewer is alive ? hide target completely
                SetMaterial(deadMaterial);
                // SetRenderersActive(false);
            }
            else
            {
                // Target is alive ? show normally to everyone
                SetMaterial(aliveMaterial);
                // SetRenderersActive(true);
            }
        }
        else
        {
            if (targetIsDead)
            {
                // Viewer is dead ? show target but faded
                SetMaterial(ghostMaterial);
                // SetRenderersActive(true);
            }
            else
            {
                // Target is alive ? show normally to everyone
                SetMaterial(aliveMaterial);
                // SetRenderersActive(true);
            }
        }
    }

    private void ReapplyVisibilityForAllPlayers()
    {
        foreach (var obj in FindObjectsOfType<PlayerState>())
        {
            obj.ApplyVisibilityForLocalViewer(obj.IsDead.Value);
        }
    }

    private void SetMaterial(Material mat)
    {
        foreach (var r in renderers)
        {
            if (r != null) r.material = mat;
        }
    }

    /*
    // Utility: enable/disable renderers
    private void SetRenderersActive(bool active)
    {
        foreach (var r in renderers)
        {
            if (r != null) r.enabled = active;
        }
    }

    // Utility: set alpha for all renderers (works for Standard shader; see note)
    private void SetRenderersAlpha(float alpha)
    {
        foreach (var r in renderers)
        {
            if (r == null) continue;

            var mats = r.materials; // this makes instance materials for this renderer
            for (int i = 0; i < mats.Length; i++)
            {
                SetMaterialAlpha(mats[i], alpha);
            }
            r.materials = mats;
        }
    }

    // Set material to transparent/opaque and apply color alpha.
    // Works reliably for Unity's built-in Standard shader.
    private void SetMaterialAlpha(Material mat, float alpha)
    {
        if (mat == null) return;

        // If material doesn't have _Color, bail (e.g., some special shaders)
        if (!mat.HasProperty("_Color"))
            return;

        Color c = mat.color;
        c.a = alpha;
        mat.color = c;

        if (alpha < 1f)
        {
            // Set up blending for Standard shader
            mat.SetFloat("_Mode", 3f); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        else
        {
            // Opaque
            mat.SetFloat("_Mode", 0f); // Opaque
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
        }
    }
    */

    // Find the local player's PlayerState (viewer). Returns null if not found yet.
    private PlayerState GetLocalPlayerState()
    {
        if (NetworkManager.Singleton == null) return null;

        var localPlayerObject = NetworkManager.Singleton.LocalClient?.PlayerObject;
        if (localPlayerObject != null)
        {
            return localPlayerObject.GetComponent<PlayerState>();
        }

        return null;
    }
}
