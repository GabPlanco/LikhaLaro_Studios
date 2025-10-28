using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class CameraAudioListenerManager : MonoBehaviour
{
    private void Awake()
    {
        // Make sure only ONE AudioListener exists
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        foreach (var l in listeners)
        {
            // If this isn't our own, disable it
            if (l.gameObject != gameObject)
                l.enabled = false;
        }
    }
}