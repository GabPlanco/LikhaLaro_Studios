using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Interactables : NetworkBehaviour
{
    // message na lumalabas pag nakatingin sa gamit
    public string promptMessage;

    // eto yung matatawag na function sa player natin
    public void BaseInteract()
    {
        Interact();
    }
    protected virtual void Interact()
    {
        // wala pa ilalagay sa ngayon
    }
}
