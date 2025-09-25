using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkCode : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI codeDisplay;

    private static NetworkCode instance;
    private static string codeText;

    private void Awake()
    {
        instance = this;

        // Re-apply the last code when a new instance spawns
        if (!string.IsNullOrEmpty(codeText))
            codeDisplay.text = codeText;
    }

    public static string CodeText
    {
        get => codeText;
        set
        {
            codeText = value;
            if (instance != null)
                instance.codeDisplay.text = codeText;
        }
    }
}