using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PlayerUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void UpdateText(string promptMessage)
    {
        if (!IsOwner) return;

        displayText.text = promptMessage;
    }
}
