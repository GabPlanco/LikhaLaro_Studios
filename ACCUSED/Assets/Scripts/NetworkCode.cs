using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkCode : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI codeDisplay;

    private static NetworkCode instance;

    private void Awake()
    {
        instance = this;
    }

    private static string codeText;

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